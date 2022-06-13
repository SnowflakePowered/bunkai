using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bunkai.Parsers.CommonParsers;
using static Pidgin.Parser;
using static Pidgin.Parser<char, string>;
using static Pidgin.Parser<char>;
using Pidgin;

using StringParser = Pidgin.Parser<char, string>;
using InfoFlagParser = Pidgin.Parser<char, Bunkai.RomInfo>;
using VersionParser = Pidgin.Parser<char, Bunkai.Tags.VersionTag>;
using TagParser = Pidgin.Parser<char, Bunkai.Tags.RomTag>;
using RegionParser = Pidgin.Parser<char, System.Collections.Generic.IEnumerable<Bunkai.Region>>;
using ManyTagParser = Pidgin.Parser<char, System.Collections.Generic.IEnumerable<Bunkai.Tags.RomTag>>;
using DumpTagParser = Pidgin.Parser<char, (Bunkai.Tags.RomTag, Bunkai.RomInfo)>;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Bunkai.Tags;

namespace Bunkai.Parsers
{
    public sealed partial class TosecParser
        : IFilenameParser, IRegionParser
    {
        private readonly static StringParser ParseZZZUnk = String("ZZZ-UNK-");

        private readonly static TagParser ParseDemoTag = InParens(String("demo").Then(Char('-').Then(OneOf(
            String("kiosk"),
            String("rolling"),
            String("playable")
            )).Optional()).Map(s => (RomTag)new ReleaseTag("demo", s.GetValueOrDefault())));

        // Parses (19|20)[0-9xX]{2}
        private readonly static StringParser ParseYear = Map((a, b) => a + b, OneOf(String("19"), 
                String("20")), OneOf(Digit, Char('X'), Char('x')).Repeat(2).Select(s => string.Concat(s)));

        // Undelimited dates must have all date components.
        private readonly static TagParser ParseUndelimitedDateTag = InParens(Map((year, month, day) =>
            (RomTag)new DateTag(year, month, day),
            Try(ParseYear),
            Digit.RepeatString(2).Assert(mstr => Int32.Parse(mstr) <= 12),
            Digit.RepeatString(2).Assert(dstr => Int32.Parse(dstr) <= 31)
        ));

        private readonly static TagParser ParseDelimitedDateTag = InParens(Map((year, month, day) =>
            (RomTag)new DateTag(year, month.GetValueOrDefault(), day.GetValueOrDefault()), 
                Try(ParseYear), Char('-').Then(Digit.RepeatString(2)).Optional(), Char('-')
            .Then(Digit.RepeatString(2)).Optional()));

        private readonly static TagParser ParseDateTag = Try(ParseUndelimitedDateTag).Or(ParseDelimitedDateTag);

        // Happy path version parsing.
        // https://github.com/SnowflakePowered/shiratsu/blob/master/shiratsu-naming/src/naming/tosec/parsers.rs#L394
        private readonly static VersionParser ParseRevisionString = String("Rev ").Then(LetterOrDigit.ManyString()).Map(s => new VersionTag("Rev", s, TagCategory.None));
        private readonly static VersionParser ParseVersionString = String("v").Then(Map((major, minor) =>
        {
            // Major version can be empty string only if minor version is not.
            // This prevents ambiguities if the title contains a v.
            if (!minor.HasValue && string.IsNullOrEmpty(major))
            {
                throw new ArgumentNullException(nameof(minor));
            }
            return new VersionTag("v", major, minor.GetValueOrDefault(), TagCategory.None);
        }, LetterOrDigit.AtLeastOnceString(), Char('.').Then(OneOf(LetterOrDigit, Char(',')).AtLeastOnceString()).Optional()));

        private readonly static TagParser ParseVersionTag = InParens(OneOf(ParseVersionString, ParseRevisionString)).Map(e => (RomTag)e);

        // https://github.com/SnowflakePowered/shiratsu/blob/master/shiratsu-naming/src/naming/tosec/parsers.rs#L427
        internal static readonly Parser<char, IEnumerable<RomTag>> VersionAndDemoOrDateHappyPathParser = from version in Try(Char(' ').Then(OneOf(ParseRevisionString, ParseVersionString))).Optional()
                                                                                     from demo in Try(Char(' ').Optional().Then(ParseDemoTag)).Optional()
                                                                                     from date in Try(Char(' ')).Optional().Then(ParseDateTag)
                                                                                     select new RomTag[] { version.GetValueOrDefault(), demo.GetValueOrDefault(), date }.Where(s => s != null);

        internal static readonly Parser<char, (string title, IEnumerable<RomTag> tags)> HappyPathTitleParser = from title in TakeUntil(Try(VersionAndDemoOrDateHappyPathParser))
                                                                                                    from stuffs in VersionAndDemoOrDateHappyPathParser
                                                                                                    select (title, stuffs);

        internal static readonly Parser<char, (string title, IEnumerable<RomTag> tags)> DegeneratePathTitleParser = from title in TakeUntil(Try(Char(' ').Before(ParseVersionString)).Or(
                                                                                                                                    Try(Char('(').Or(Char('[')))))
                                                                                                                    from version in Char(' ').Then(ParseVersionString).Optional()
                                                                                                                    select (title, version.Match(v => new RomTag[] { v }, () => Enumerable.Empty<RomTag>()));

        internal static readonly StringParser ParseByPublisher = from _ in String(" by ") 
                                                                 from publisher in Any.Until(Lookahead(Char('(').Or(End.WithResult(' ')))).Select(string.Concat)
                                                                 select publisher;

        internal static readonly TagParser ParseParensTextTag = InParens(TakeUntil(Char(')'))).Map(s => (RomTag)new TextTag(s, TagCategory.Parenthesized));

        internal static readonly TagParser ParseMoreInfoTextTag = InBrackets(TakeUntil(Char(']')).Select(string.Concat)).Map(s => (RomTag)new TextTag(s, TagCategory.Parenthesized));


        internal static readonly RegionParser ParseRegionTag = InParens(Letter.Or(Char('-')).ManyString()).Map(regionString => ParseRegion(regionString).AsEnumerable()).Assert(e => e.Any());

        internal static readonly TagParser ParseMultilanguageTag = InParens(Map((_, digits) => (RomTag)new MultiLanguageTag(digits), String("M"), Digit.ManyString().Map(s => Int32.Parse(s))));

        internal static readonly TagParser ParseSingleLanguageTag = InParens(Lowercase.RepeatString(2).Map(l => (RomTag)new LanguageTag(RegionMap.LANGUAGE_MAP[l])));

        internal static readonly TagParser ParseDoubleLanguageTag = InParens(Map((l1, l2) => (RomTag)new LanguageTag(RegionMap.LANGUAGE_MAP[l1], RegionMap.LANGUAGE_MAP[l2]),
            Lowercase.RepeatString(2), Lowercase.RepeatString(2)));

        internal static DumpTagParser DumpInfoParser(StringParser tagParser) => InBrackets(from tag in tagParser
                                                                                       from index in Digit.AtLeastOnceString().Optional()
                                                                                       from param in Char(' ').Then(TakeUntil(Char(']')).Select(s => string.Concat(s))).Optional()
                                                                                       select ((RomTag)new TextTag(tag + index.GetValueOrDefault("") + param.Match(s => " " + s, () => ""), "Dumpflag", TagCategory.Bracketed), MatchDumpFlag(tag))
                                                                                       );
        internal static Parser<char, (IEnumerable<RomTag>, RomInfo)> ParseKnownDumpTags = from _ in Char(' ').Optional()
                                                                                          from dumpinfos in OneOf(
                                                                                              Try(DumpInfoParser(String("cr"))),
                                                                                              Try(DumpInfoParser(String("f"))),
                                                                                              Try(DumpInfoParser(String("h"))),
                                                                                              Try(DumpInfoParser(String("m"))),
                                                                                              Try(DumpInfoParser(String("p"))),
                                                                                              Try(DumpInfoParser(String("t"))),
                                                                                              Try(DumpInfoParser(String("tr"))),
                                                                                              Try(DumpInfoParser(String("t"))),
                                                                                              Try(DumpInfoParser(String("o"))),
                                                                                              Try(DumpInfoParser(String("u"))),
                                                                                              Try(DumpInfoParser(String("v"))),
                                                                                              Try(DumpInfoParser(String("b"))),
                                                                                              Try(DumpInfoParser(String("a"))),
                                                                                              Try(DumpInfoParser(String("!")))).AtLeastOnce()
                                                                                          select (dumpinfos.Select(d => d.Item1), MergeRomInfo(dumpinfos.Select(d => d.Item2)));

        // https://github.com/SnowflakePowered/shiratsu/blob/master/shiratsu-naming/src/naming/tosec/parsers.rs#L264
        // Bunkai doesn't care about the semantics of the system tag so it's just a texttag.
        internal static readonly TagParser ParseSystemTag = InParens(OneOf(
            Try(String("+2")),
            Try(String("+2a")),
            Try(String("+3")),
            Try(String("130XE")),
            Try(String("A1000")),
            Try(String("A1200")),
            Try(String("A1200-A4000")),
            Try(String("A2000")),
            Try(String("A2000-A3000")),
            Try(String("A2024")),
            Try(String("A2500-A3000UX")),
            Try(String("A3000")),
            Try(String("A4000")),
            Try(String("A4000T")),
            Try(String("A500")),
            Try(String("A500+")),
            Try(String("A500-A1000-A2000")),
            Try(String("A500-A1000-A2000-CDTV")),
            Try(String("A500-A1200")),
            Try(String("A500-A1200-A2000-A4000")),
            Try(String("A500-A2000")),
            Try(String("A500-A600-A2000")),
            Try(String("A570")),
            Try(String("A600")),
            Try(String("A600HD")),
            Try(String("AGA")),
            Try(String("AGA-CD32")),
            Try(String("Aladdin Deck Enhancer")),
            Try(String("CD32")),
            Try(String("CDTV")),
            Try(String("Computrainer")),
            Try(String("Doctor PC Jr.")),
            Try(String("ECS")),
            Try(String("ECS-AGA")),
            Try(String("Executive")),
            Try(String("Mega ST")),
            Try(String("Mega-STE")),
            Try(String("OCS")),
            Try(String("OCS-AGA")),
            Try(String("ORCH80")),
            Try(String("Osbourne 1")),
            Try(String("PIANO90")),
            Try(String("PlayChoice-10")),
            Try(String("Plus4")),
            Try(String("Primo-A")),
            Try(String("Primo-A64")),
            Try(String("Primo-B")),
            Try(String("Primo-B64")),
            Try(String("Pro-Primo")),
            Try(String("ST")),
            Try(String("STE")),
            Try(String("STE-Falcon")),
            Try(String("TT")),
            Try(String("TURBO-R GT")),
            Try(String("TURBO-R ST")),
            Try(String("VS DualSystem")),
            Try(String("VS UniSystem"))
            ).Map(s => (RomTag)new TextTag(s, "System", TagCategory.Parenthesized)));

        internal static readonly TagParser ParseVideoTag = InParens(OneOf(
            Try(String("CGA")),
            Try(String("EGA")),
            Try(String("HGC")),
            Try(String("MCGA")),
            Try(String("MDA")),
            Try(String("NTSC")),
            Try(String("NTSC-PAL")),
            Try(String("PAL")),
            Try(String("PAL-60")),
            Try(String("PAL-NTSC")),
            Try(String("SVGA")),
            Try(String("VGA")),
            Try(String("XGA"))
            ).Map(s => (RomTag)new TextTag(s, "Video", TagCategory.Parenthesized)));

        internal static readonly TagParser ParseCopyrightTag = InParens(OneOf(
            Try(String("CW")),
            Try(String("CW-R")),
            Try(String("FW")),
            Try(String("GW")),
            Try(String("GW-R")),
            Try(String("LW")),
            Try(String("PD")),
            Try(String("SW")),
            Try(String("SW-R"))
            ).Map(s => (RomTag)new TextTag(s, "Copyright", TagCategory.Parenthesized)));

        internal static readonly TagParser ParseDevStatusTag = InParens(OneOf(
            Try(String("alpha")),
            Try(String("beta")),
            Try(String("preview")),
            Try(String("pre-release")),
            Try(String("proto")),

            // these are malformed but acceptable.
            // see https://github.com/SnowflakePowered/shiratsu/blob/master/shiratsu-naming/src/naming/tosec/parsers.rs#L57 for strict parsing.
            Try(String("Alpha")),
            Try(String("Beta")),
            Try(String("Preview")),
            Try(String("Pre-Release")),
            Try(String("Proto")),
            Try(String("Prototype"))
            )).Map(s => (RomTag)new ReleaseTag(s, null));

        internal static readonly TagParser ParseMediaTag = InParens(Map((type, part, total, side) => (RomTag)new MediaTag(type, part, total.GetValueOrDefault(), side.GetValueOrDefault()),
          OneOf(
              Try(String("Disk")),
              Try(String("Disc")),
              Try(String("File")),
              Try(String("Part")),
              Try(String("Side"))).Before(Char(' ')),
          LetterOrDigit.Or(Char('-')).AtLeastOnceString(),
          String(" of ").Then(LetterOrDigit.Or(Char('-')).AtLeastOnceString()).Optional(),
          String(" Side ").Then(LetterOrDigit.AtLeastOnceString()).Optional()));

        internal static readonly TagParser ParseKnownTag = OneOf(
            Try(ParseMultilanguageTag),
            Try(ParseSingleLanguageTag),
            Try(ParseDoubleLanguageTag),
            Try(ParseSystemTag),
            Try(ParseVideoTag),
            Try(ParseCopyrightTag),
            Try(ParseMediaTag),
            // todo: goodtools region
            Try(ParseDevStatusTag),
            Try(ParseVersionTag)
            );

    
        internal static readonly Parser<char, NameInfo> NameParser = from zzz in ParseZZZUnk.Optional() // ZZZ-UNK- is just ignored.
                                                                                   from res in OneOf(Try(HappyPathTitleParser),
                                                                                                     Try(DegeneratePathTitleParser),
                                                                                                     Any.ManyString().Map<(string title, IEnumerable<RomTag> tags)>(s => (s, Enumerable.Empty<RomTag>())))
                                                                                                .Bind<(string title, IEnumerable<RomTag> tags)>((tuple) =>
                                                                                                {
                                                                                                (string title, IEnumerable<RomTag> titleTags) = tuple;
                                                                                                    // ZZZ-UNK triggers special path for old-style by-publisher
                                                                                                    if (zzz.HasValue)
                                                                                                    {
                                                                                                        // case 1: publisher is part of title
                                                                                                        // unlike shiratsu-naming we don't care about spaces.
                                                                                                        var result = Map((title, publisher) => (title, titleTags: titleTags.Append(new PublisherTag(publisher.TrimEnd(), TagCategory.Parenthesized))),
                                                                                                            TakeUntil(Try(String(" by "))),
                                                                                                            ParseByPublisher).Parse(title);
                                                                                                        if (result.Success) {
                                                                                                            return FromResult((result.Value.title, result.Value.titleTags));
                                                                                                        }

                                                                                                        // case 2: publisher is past tags with by
                                                                                                        // unlike shiratsu-naming we don't care about spaces.
                                                                                                        return (from parsedPublisher in Try(ParseByPublisher).Optional()
                                                                                                            let newTags = parsedPublisher.Match(j => titleTags.Append(new PublisherTag(j.TrimEnd(), TagCategory.Parenthesized)), () => titleTags)
                                                                                                            select (title, newTags));

                                                                                                    // above case allows for missing publisher.
                                                                                                    }

                                                                                                    return FromResult((title, titleTags));
                                                                                                   
                                                                                                })
                                                                                   let title = res.title
                                                                                   let titleTags = res.tags
                                                                                   from _0 in Char(' ').SkipMany().Optional() // possible unexpected space


                                                                                   from rest in Any.ManyString().Map(rest =>
                                                                                   {
                                                                                       var preparser = (titleTags.Any(t => t is PublisherTag)
                                                                                            || OneOf(ParseKnownTag, Try(ParseMoreInfoTextTag)).Parse(rest).Success)
                                                                                                // case 1: publisher already parsed or is missing
                                                                                                ? from _0 in Char(' ').Optional()
                                                                                                  from regions in Try(ParseRegionTag).Optional()
                                                                                                  from _1 in Char(' ').Optional()
                                                                                                  from tags in OneOf(ParseKnownTag, Try(ParseParensTextTag)).Many()
                                                                                                  from dumpinfos in Try(ParseKnownDumpTags).Optional()
                                                                                                  from bracktags in Try(ParseMoreInfoTextTag).Many().Optional()
                                                                                                  let dumpinfo_res = dumpinfos.GetValueOrDefault((Enumerable.Empty<RomTag>(), RomInfo.None))
                                                                                                  let bracktags_res = bracktags.GetValueOrDefault(Enumerable.Empty<RomTag>())
                                                                                                  let final_tags = regions.Match(r => new[] { new RegionTag(r.ToArray()) }, () => Enumerable.Empty<RomTag>()).Concat(tags)
                                                                                                    .Concat(dumpinfo_res.Item1)
                                                                                                    .Concat(bracktags_res)
                                                                                                  select (tags: final_tags, info: dumpinfo_res.Item2)
                                                                                                // case 2: need to parse publisher, unlike shiratsu-naming all the publishers get mushed into one.
                                                                                                : (from publisher in InParens(LetterOrDigit.Or(Char('-')).AtLeastOnceString()).Map(publisher => new PublisherTag(publisher, TagCategory.Parenthesized))
                                                                                                   from _ in Char(' ').SkipMany().Optional()
                                                                                                   from regions in Try(ParseRegionTag).Optional()
                                                                                                   from tags in Try(Char(' ').Optional().Then(ParseKnownTag)).Many()
                                                                                                   from dumpinfos in Try(ParseKnownDumpTags).Optional()
                                                                                                   from bracktags in Try(ParseMoreInfoTextTag).Many().Optional()
                                                                                                   let dumpinfo_res = dumpinfos.GetValueOrDefault((Enumerable.Empty<RomTag>(), RomInfo.None))
                                                                                                   let bracktags_res = bracktags.GetValueOrDefault(Enumerable.Empty<RomTag>())
                                                                                                   let final_tags = regions.Match(r => new[] { new RegionTag(r.ToArray()) }, () => Enumerable.Empty<RomTag>())
                                                                                                    .Concat(tags.Prepend(publisher))
                                                                                                    .Concat(dumpinfo_res.Item1)
                                                                                                    .Concat(bracktags_res)
                                                                                                   select (tags: final_tags, info: dumpinfo_res.Item2));

                                                                                       return preparser.Parse(rest);
                                                                                   })
                                                                                   
                                                                                   let result = rest.GetValueOrDefault((Enumerable.Empty<RomTag>(), RomInfo.None))
                                                                                   let finalTags = titleTags.Concat(result.tags).ToImmutableArray()
                                                                     select new NameInfo(NamingConvention.TheOldSchoolEmulationCenter, title.Trim(), finalTags, CombineRomInfo(finalTags, result.info));


    }
}
