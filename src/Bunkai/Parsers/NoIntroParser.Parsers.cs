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
using ManyTagParser = Pidgin.Parser<char, System.Collections.Generic.IEnumerable<Bunkai.Tags.RomTag>>;

using RegionParser = Pidgin.Parser<char, System.Collections.Generic.IEnumerable<Bunkai.Region>>;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Bunkai.Tags;

namespace Bunkai.Parsers
{
    public sealed partial class NoIntroParser
         : IFilenameParser, IRegionParser
    {
        internal static readonly TagParser ParseBiosTag = InBrackets(String("BIOS"))
            .ThenReturn<RomTag>(new TextTag("BIOS", TagCategory.Bracketed));

        internal static readonly TagParser ParseSceneTag = OneOf(
            Digit.RepeatString(4).Map<RomTag>(s => new SceneTag(s, null)),
            String("xB").Then(Digit.RepeatString(2)).Map<RomTag>(s => new SceneTag(s, "xB")),
            Map((prefix, number) => (RomTag)new SceneTag(number, prefix.ToString()), CIOneOf('x', 'z'), Digit.RepeatString(3)))
        .Before(String(" - "));

        internal static readonly TagParser ParseRedumpMultitapTag = InParens(Sequence(String("Multi Tap ("), TakeUntil(String(")")), String(")"), TakeUntil(String(")"))).Map(s => string.Concat(s)))
                    .Map<RomTag>(s => new TextTag(s, TagCategory.Parenthesized));

        internal static readonly StringParser RegionKey = OneOf(RegionMap.NOINTRO_MAP.Keys.Select(s => Try(String(s)))
                                                           .Concat(new[] { Try(String("World")),
                                                                Try(String("Latin America")),
                                                                Try(String("Scandinavia")) }));

        internal static readonly TagParser ParseAdditionalFlag = InParens(Any.AtLeastOnceUntil(Lookahead(Char(')')))
            .Select(cs => string.Concat(cs))).Map<RomTag>(s => new TextTag(s, TagCategory.Parenthesized));

        internal static readonly TagParser ParseBadDumpTag = InBrackets(String("b"))
                                                .ThenReturn<RomTag>(new TextTag("b", TagCategory.Bracketed));


        internal static readonly TagParser ParseReleaseTag = InParens(
            Map((status, meta) => (RomTag)new ReleaseTag(status, meta.HasValue ? meta.Value : null),
                OneOf(
                    Try(String("Demo")),
                    Try(String("Beta")),
                    Try(String("Sample")),
                    Try(String("Prototype")),
                    Try(String("Proto"))
                ),
                Char(' ').Then(LetterOrDigit.Or(Whitespace).AtLeastOnceString()).Optional()
                ));

        internal static readonly TagParser ParseDiscTag = InParens(
            String("Disc ").Then(Digit).Map(d => (RomTag)new MediaTag("Disc", d.ToString()))
            );

        internal static readonly RegionParser ParseRegionTag = InParens(RegionKey.Map(x => ParseRegion(x))
            .Separated(String(", "))).Map(x => x.SelectMany(r => r).Distinct()).Assert(r => r.Any());
        internal static readonly TagParser ParseRegionTagAndEnsureEnd = ParseRegionTag.Before(OneOf(
                Try(End),
                Try(Lookahead(Char(' ').Then(
                    ParseBadDumpTag.ThenReturn(Unit.Value).Or(
                    ParseAdditionalFlag.ThenReturn(Unit.Value))
                )))
            )).Map(r => (RomTag)new RegionTag(r.ToArray()));

        internal static readonly Parser<char, Unit> ParseIsoLanguageVariant = Char('-').Then(Letter.SkipAtLeastOnce());

        internal static readonly StringParser ParseIsoLanguageCode = Letter.RepeatString(2).Where(s => RegionMap.LANGUAGE_MAP.ContainsKey(s.ToLowerInvariant()))
            .Before(ParseIsoLanguageVariant.Optional());

        internal static readonly TagParser ParseLanguageTag = InParens(
            ParseIsoLanguageCode.SeparatedAtLeastOnce(Char(',')).Map(s => (RomTag)new LanguageTag(s.Select(s => RegionMap.LANGUAGE_MAP[s.ToLowerInvariant()]).ToArray())));

        internal static readonly VersionParser ParseRevisionVersion = String("Rev ")
            .Then(Map((major, minor) => minor.Match(
                                                    minor => new VersionTag("Rev", major, minor, TagCategory.Parenthesized), 
                                                    () => new VersionTag("Rev", major, TagCategory.Parenthesized)),

                LetterOrDigit.AtLeastOnceString(), Char('.').Then(LetterOrDigit.AtLeastOnceString()).Optional()));

        internal static readonly VersionParser ParseSinglePrefixedVersion = String("v")
            .Then(Map((major, minor, suffix) => new VersionTag("v", string.Concat(major), minor.GetValueOrDefault(), suffix.GetValueOrDefault(), TagCategory.Parenthesized),
                Digit.AtLeastOnce(),
                Char('.').Then(OneOf(LetterOrDigit, Char('.'), Char('-')).ManyString()).Optional(),
                Char(' ').Then(String("Alt")).Optional()
            ));


        internal static readonly VersionParser ParseSinglePrefixedVersionWithFullSuffix = String("Version ")
            .Then(Map((major, minor, suffix) => new VersionTag("Version", string.Concat(major), minor.GetValueOrDefault(), suffix.GetValueOrDefault(), TagCategory.Parenthesized),
                Digit.AtLeastOnce(),
                Char('.').Then(OneOf(LetterOrDigit, Char('.'), Char('-')).ManyString()).Optional(),
                Char(' ').Then(
                    OneOf(
                        Try(String("Alt")),
                        Try(Uppercase).Map(s => s.ToString())
                        )
                ).Many().Optional()
            ));
        internal static readonly VersionParser ParseUnprefixedDotVersion = 
                Map((major, minor) => new VersionTag("", major.ToString(), minor.ToString(), TagCategory.Parenthesized),
                    Digit,
                    Char('.').Then(Digit)
                   );

        internal static readonly ManyTagParser ParseVersionTag = InParens(OneOf(
            Try(ParseSinglePrefixedVersion),
            Try(ParseSinglePrefixedVersionWithFullSuffix),
            Try(ParseRevisionVersion),
            Try(ParseUnprefixedDotVersion)
            ).SeparatedAtLeastOnce(String(", ")).Map(tag => tag.Select(t => (RomTag)t)));

        private static ManyTagParser ParseKnownTags = Char(' ').Optional().Then(OneOf(
            Try(ParseLanguageTag.AtLeastOnce()),
            Try(ParseVersionTag),
            Try(ParseReleaseTag.AtLeastOnce()),
            Try(ParseDiscTag.AtLeastOnce()),
            Try(ParseRedumpMultitapTag.AtLeastOnce()),
            Try(ParseAdditionalFlag.AtLeastOnce())
            ));

        internal static readonly Parser<char, NameInfo> NameParser = from scene in ParseSceneTag.Optional()
                                                                     from bios in ParseBiosTag.Optional()
                                                                     from title in TakeUntil(Try(ParseRegionTagAndEnsureEnd))
                                                                     from region in ParseRegionTagAndEnsureEnd

                                                                    from restTags in Try(ParseKnownTags).Many()
                                                                    from badDump in Char(' ').Optional().Then(ParseBadDumpTag).Optional()
                                                                    let tags = MergeTags(restTags, bios, scene, Maybe.Just(region), badDump)
                                                                    from _eof in End
                                                                    select new NameInfo(NamingConvention.NoIntro, title.Trim(), tags,
                                                                      MergeInfoFlags(tags));

    }
}
