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
    public sealed partial class TosecParser
        : IFilenameParser
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
        private readonly static TagParser ParseUndelimitedDate = InParens(Map((year, month, day) =>
            (RomTag)new DateTag(year, month, day),
            Try(ParseYear),
            Digit.RepeatString(2).Assert(mstr => Int32.Parse(mstr) <= 12),
            Digit.RepeatString(2).Assert(dstr => Int32.Parse(dstr) <= 31)
        ));

        private readonly static TagParser ParseDelimitedDate = InParens(Map((year, month, day) =>
            (RomTag)new DateTag(year, month.GetValueOrDefault(), day.GetValueOrDefault()), 
                Try(ParseYear), Char('-').Then(Digit.RepeatString(2)).Optional(), Char('-')
            .Then(Digit.RepeatString(2)).Optional()));

        private readonly static TagParser ParseDate = Try(ParseUndelimitedDate).Or(ParseDelimitedDate);

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

        // https://github.com/SnowflakePowered/shiratsu/blob/master/shiratsu-naming/src/naming/tosec/parsers.rs#L427
        internal static readonly Parser<char, IEnumerable<RomTag>> VersionAndDemoOrDateHappyPathParser = from version in Try(Char(' ').Then(OneOf(ParseRevisionString, ParseVersionString))).Optional()
                                                                                     from demo in Try(Char(' ').Optional().Then(ParseDemoTag)).Optional()
                                                                                     from date in Try(Char(' ')).Optional().Then(ParseDate)
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


        // does not handle the 
        internal static readonly Parser<char, NameInfo> NameParser = from zzz in ParseZZZUnk.Optional() // ZZZ-UNK- is just ignored.
                                                                                   from res in OneOf(Try(HappyPathTitleParser),
                                                                                                     Try(DegeneratePathTitleParser),
                                                                                                     Any.ManyString().Map<(string title, IEnumerable<RomTag> tags)>(s => (s, Enumerable.Empty<RomTag>())))
                                                                                                .Bind<(string title, IEnumerable<RomTag> tags)>((tuple) =>
                                                                                                {
                                                                                                (string title, IEnumerable<RomTag> titleTags) = tuple;
                                                                                                if (zzz.HasValue)
                                                                                                {
                                                                                                    // case 1: publisher is part of title

                                                                                                    var result = Map((title, publisher) => (title, titleTags: titleTags.Append(new PublisherTag(publisher.TrimEnd(), TagCategory.Parenthesized))),
                                                                                                        TakeUntil(Try(String(" by "))),
                                                                                                        ParseByPublisher).Parse(title);
                                                                                                        if (result.Success)
                                                                                                            return FromResult((result.Value.title, result.Value.titleTags));

                                                                                                    // case 2: publisher is past tags with by
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

                                                                                   from rest in Any.ManyString()
                                                                                 
                                                                     select new NameInfo(NamingConvention.TheOldSchoolEmulationCenter, title.Trim(), titleTags.ToImmutableArray(), RomInfo.None);

        public bool TryParse(string filename, [NotNullWhen(true)] out NameInfo? nameInfo)
        {
            var res = NameParser.Parse(filename);
            nameInfo = res.Success ? res.Value : null;
            return res.Success;
        }
    }
}
