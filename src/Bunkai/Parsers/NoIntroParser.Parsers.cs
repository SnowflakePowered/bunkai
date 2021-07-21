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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Bunkai.Tags;

namespace Bunkai.Parsers
{
    public sealed partial class NoIntroParser
         : IFilenameParser, IRegionParser
    {
        private static TagParser ParseBiosTag = InBrackets(String("BIOS"))
            .ThenReturn<RomTag>(new TextTag("BIOS", TagCategory.Bracketed));

        private static TagParser ParseSceneTag = OneOf(
            Digit.RepeatString(4).Map<RomTag>(s => new SceneTag(s, null)),
            String("xB").Then(Digit.RepeatString(2)).Map<RomTag>(s => new SceneTag(s, "xB")),
            Map((prefix, number) => (RomTag)new SceneTag(number, prefix.ToString()), CIOneOf('x', 'z'), Digit.RepeatString(3)))
        .Before(String(" - "));

        private static TagParser ParseRedumpMultitapTag = InParens(Sequence(String("Multi Tap ("), TakeUntil(String(")")), String(")"), TakeUntil(String(")"))).Map(s => string.Concat(s)))
                    .Map<RomTag>(s => new TextTag(s, TagCategory.Parenthesized));

        VersionParser ParseRevision = InParens(String("Rev ")
            .Then(DecimalNum).Map(v => new VersionTag("Rev", v.ToString())));


        //VersionParser ParseVersion = InParens(String("v")
        //                                .Or(String("Version "))
        //                                .Then(DecimalNum).
        //    .Then(DecimalNum).Map(v => new Version("Rev", v.ToString())));

        private static readonly StringParser RegionKey = OneOf(RegionMap.NOINTRO_MAP.Keys.Select(s => Try(String(s)))
                                                           .Concat(new[] { Try(String("World")),
                                                                Try(String("Latin America")),
                                                                Try(String("Scandinavia")) }));
        private static readonly TagParser ParseAdditionalFlag = InParens(Any.AtLeastOnceUntil(Lookahead(Char(')')))
            .Select(cs => string.Concat(cs))).Map<RomTag>(s => new TextTag(s, TagCategory.Parenthesized));

        private static TagParser ParseBadDumpTag = InBrackets(String("b"))
                                                .ThenReturn<RomTag>(new TextTag("b", TagCategory.Bracketed));


        private static TagParser ParseReleaseTag = InParens(
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


        private static readonly RegionParser ParseRegionTag = InParens(RegionKey.Map(x => ParseRegion(x)).Separated(String(", "))).Map(x => x.SelectMany(r => r).Distinct());
        private static readonly RegionParser ParseRegionTagAndEnsureEnd = ParseRegionTag.Before(OneOf(
                Try(End),
                Try(Lookahead(Char(' ').Then(
                    ParseBadDumpTag.ThenReturn(Unit.Value).Or(
                    ParseAdditionalFlag.ThenReturn(Unit.Value))
                )))
            ));



        private static TagParser ParseKnownTags = Char(' ').Optional().Then(OneOf(
             // Try(ParseLanguageTag),
             // Try(ParseVersionTag),
            Try(ParseReleaseTag),
            // Try(ParseDiscTag),
            Try(ParseRedumpMultitapTag),
            Try(ParseAdditionalFlag)
            ));

        private static readonly Parser<char, NameInfo> NameParser = from scene in ParseSceneTag.Optional()
                                                                    from bios in ParseBiosTag.Optional()
                                                                    from title in Any.AtLeastOnceUntil(Lookahead(Try(ParseRegionTagAndEnsureEnd))).Select(s => string.Concat(s))
                                                                    from region in ParseRegionTagAndEnsureEnd
                                                                    from restTags in Try(ParseKnownTags).Many()
                                                                    from badDump in Char(' ').Optional().Then(ParseBadDumpTag).Optional()
                                                                    let tags = MergeTags(restTags, bios, scene, badDump)
                                                                    from _eof in End
                                                                    select new NameInfo(NamingConvention.NoIntro, title.Trim(), region.ToImmutableArray(), tags,
                                                                      MergeInfoFlags(tags));

    }
}
