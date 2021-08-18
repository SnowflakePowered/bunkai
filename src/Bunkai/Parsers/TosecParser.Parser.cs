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

        private readonly static VersionParser ParseRevision = String("Rev ").Then(LetterOrDigit.ManyString()).Map(s => new VersionTag("Rev", s, TagCategory.None));
        private readonly static VersionParser ParseVersion = String("v").Then(Map((major, minor) =>
        {
            // Major version can be empty string only if minor version is not.
            // This prevents ambiguities if the title contains a v.
            if (!minor.HasValue && string.IsNullOrEmpty(major))
            {
                throw new ArgumentNullException(nameof(minor));
            }
            return new VersionTag("v", major, minor.GetValueOrDefault(), TagCategory.None);
        }, LetterOrDigit.AtLeastOnceString(), Char('.').Then(OneOf(LetterOrDigit, Char(',')).AtLeastOnceString()).Optional()));

        private readonly static StringParser ParseYear = Map((a, b) => a + b, OneOf(String("19"), String("20")), OneOf(Digit, Char('X'), Char('x')).Repeat(2).Select(s => string.Concat(s)));
        private readonly static TagParser ParseDate = InParens(Map((year, month, day) => (RomTag)new DateTag(year, month.GetValueOrDefault(), day.GetValueOrDefault()), Try(ParseYear), Char('-').Then(Digit.RepeatString(2)).Optional(), Char('-').Then(Digit.RepeatString(2)).Optional()));

        internal static readonly Parser<char, IEnumerable<RomTag>> VersionAndDemoOrDateParser = from version in Try(Char(' ').Then(OneOf(ParseRevision, ParseVersion))).Optional()
                                                                                     from demo in Try(Char(' ').Optional().Then(ParseDemoTag)).Optional()
                                                                                     from date in Try(Char(' ')).Optional().Then(ParseDate)
                                                                                     select new RomTag[] { version.GetValueOrDefault(), demo.GetValueOrDefault(), date }.Where(s => s != null);

        internal static readonly Parser<char, NameInfo> NameParser = from _ in ParseZZZUnk.Optional()
                                                                     from title in Any.AtLeastOnceUntil(Lookahead(Try(VersionAndDemoOrDateParser))).Select(cs => string.Concat(cs))
                                                                     from stuffs in VersionAndDemoOrDateParser
                                                                     select new NameInfo(NamingConvention.TheOldSchoolEmulationCenter, title.Trim(), stuffs.ToImmutableArray(), RomInfo.None);

        public bool TryParse(string filename, [NotNullWhen(true)] out NameInfo? nameInfo)
        {
            var res = NameParser.Parse(filename);
            nameInfo = res.Success ? res.Value : null;
            return res.Success;
        }
    }
}
