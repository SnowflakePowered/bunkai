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


        internal static readonly RegionParser ParseRegionTag = InParens(TakeUntil(Char(')'))).Map(regionString => ParseRegion(regionString).AsEnumerable()).Assert(e => e.Any());

        internal static readonly TagParser ParseMultilanguageTag = InParens(Map((_, digits) => (RomTag)new MultiLanguageTag(digits), String("M"), Digit.ManyString().Map(s => Int32.Parse(s))));

        internal static readonly TagParser ParseSingleLanguageTag = InParens(Lowercase.RepeatString(2).Map(l => (RomTag)new LanguageTag(RegionMap.LANGUAGE_MAP[l])));

        internal static readonly TagParser ParseDoubleLanguageTag = InParens(Map((l1, l2) => (RomTag)new LanguageTag(RegionMap.LANGUAGE_MAP[l1], RegionMap.LANGUAGE_MAP[l2]),
            Lowercase.RepeatString(2), Lowercase.RepeatString(2)));
        // todo: single, double language


        // https://github.com/SnowflakePowered/shiratsu/blob/master/shiratsu-naming/src/naming/tosec/parsers.rs#L264
        // Bunkai doesn't care about the semantics of the system tag so it's just a texttag.
        internal static readonly TagParser ParseSystemTag = InParens(OneOf(
            String("+2"),
            String("+2a"),
            String("+3"),
            String("130XE"),
            String("A1000"),
            String("A1200"),
            String("A1200-A4000"),
            String("A2000"),
            String("A2000-A3000"),
            String("A2024"),
            String("A2500-A3000UX"),
            String("A3000"),
            String("A4000"),
            String("A4000T"),
            String("A500"),
            String("A500+"),
            String("A500-A1000-A2000"),
            String("A500-A1000-A2000-CDTV"),
            String("A500-A1200"),
            String("A500-A1200-A2000-A4000"),
            String("A500-A2000"),
            String("A500-A600-A2000"),
            String("A570"),
            String("A600"),
            String("A600HD"),
            String("AGA"),
            String("AGA-CD32"),
            String("Aladdin Deck Enhancer"),
            String("CD32"),
            String("CDTV"),
            String("Computrainer"),
            String("Doctor PC Jr."),
            String("ECS"),
            String("ECS-AGA"),
            String("Executive"),
            String("Mega ST"),
            String("Mega-STE"),
            String("OCS"),
            String("OCS-AGA"),
            String("ORCH80"),
            String("Osbourne 1"),
            String("PIANO90"),
            String("PlayChoice-10"),
            String("Plus4"),
            String("Primo-A"),
            String("Primo-A64"),
            String("Primo-B"),
            String("Primo-B64"),
            String("Pro-Primo"),
            String("ST"),
            String("STE"),
            String("STE-Falcon"),
            String("TT"),
            String("TURBO-R GT"),
            String("TURBO-R ST"),
            String("VS DualSystem"),
            String("VS UniSystem")
            ).Map(s => (RomTag)new TextTag(s, "System", TagCategory.Parenthesized)));

        internal static readonly TagParser ParseVideoTag = InParens(OneOf(
            String("CGA"),
            String("EGA"),
            String("HGC"),
            String("MCGA"),
            String("MDA"),
            String("NTSC"),
            String("NTSC-PAL"),
            String("PAL"),
            String("PAL-60"),
            String("PAL-NTSC"),
            String("SVGA"),
            String("VGA"),
            String("XGA")
            ).Map(s => (RomTag)new TextTag(s, "Video", TagCategory.Parenthesized)));

        internal static readonly TagParser ParseCopyrightTag = InParens(OneOf(
            String("CW"),
            String("CW-R"),
            String("FW"),
            String("GW"),
            String("GW-R"),
            String("LW"),
            String("PD"),
            String("SW"),
            String("SW-R")
            ).Map(s => (RomTag)new TextTag(s, "Copyright", TagCategory.Parenthesized)));

        internal static readonly TagParser ParseDevStatusTag = InParens(OneOf(
            String("alpha"),
            String("beta"),
            String("preview"),
            String("pre-release"),
            String("proto"),

            // these are malformed but acceptable.
            // see https://github.com/SnowflakePowered/shiratsu/blob/master/shiratsu-naming/src/naming/tosec/parsers.rs#L57 for strict parsing.
            String("Alpha"),
            String("Beta"),
            String("Preview"),
            String("Pre-Release"),
            String("Proto"),
            String("Prototype")
            )).Map(s => (RomTag)new ReleaseTag(s, null));

        internal static readonly TagParser ParseKnownTag = OneOf(
            Try(ParseMultilanguageTag),
            Try(ParseSingleLanguageTag),
            Try(ParseDoubleLanguageTag),
            Try(ParseSystemTag),
            Try(ParseVideoTag),
            Try(ParseCopyrightTag),
            // todo: media
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
                                                                                   let _ = Char(' ').Optional() // possible unexpected space


                                                                                   from rest in Any.ManyString().Map(rest =>
                                                                                   {
                                                                                       var preparser = (titleTags.Any(t => t is PublisherTag) 
                                                                                            || OneOf(ParseKnownTag, Try(ParseMoreInfoTextTag)).Parse(rest).Success)
                                                                                                // case 1: publisher already parsed or is missing
                                                                                                ? Char(' ').Optional().Then(OneOf(ParseKnownTag, Try(ParseParensTextTag))).Many()
                                                                                                // case 2: need to parse publisher (todo: properly parse publisher)
                                                                                                : (from publisher in ParseParensTextTag
                                                                                                   from tags in Char(' ').Optional().Then(ParseKnownTag).Many()
                                                                                                   select tags.Prepend(publisher));


                                                                                       return preparser.Parse(rest);
                                                                                   })

                                                                                   
                                                                     select new NameInfo(NamingConvention.TheOldSchoolEmulationCenter, title.Trim(), titleTags.Concat(rest
                                                                        .GetValueOrDefault(Enumerable.Empty<RomTag>())).ToImmutableArray(), RomInfo.None);

        public bool TryParse(string filename, [NotNullWhen(true)] out NameInfo? nameInfo)
        {
            var res = NameParser.Parse(filename);
            nameInfo = res.Success ? res.Value : null;
            return res.Success;
        }
    }
}
