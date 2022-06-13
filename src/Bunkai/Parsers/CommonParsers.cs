using Pidgin;
using static Pidgin.Parser<char>;
using static Pidgin.Parser;
using static Pidgin.Parser<char, string>;
using StringParser = Pidgin.Parser<char, string>;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace Bunkai.Parsers
{
    internal static class CommonParsers
    {
      
        public static StringParser OpenParens = String("(");
        public static StringParser CloseParens = String(")");
        public static Parser<char, T> InParens<T>(Parser<char, T> inner) =>
            inner.Between(OpenParens, CloseParens);

        public static StringParser OpenBracket = String("[");
        public static StringParser CloseBracket = String("]");

        public static StringParser TakeUntil<T>(Parser<char, T> inner) => Any.AtLeastOnceUntil(Lookahead(inner)).Select(s => string.Concat(s));
        public static Parser<char, T> InBrackets<T>(Parser<char, T> inner) =>
            inner.Between(OpenBracket, CloseBracket);


        private static IFilenameParser noIntroNameParser = new NoIntroParser();
        public static bool TryParseFileName(string filename, bool stripFileext, [NotNullWhen(true)] out NameInfo? nameInfo)
        {
            if (stripFileext)
                filename = Path.GetFileNameWithoutExtension(filename).Trim();

            if (noIntroNameParser.TryParse(filename, out nameInfo))
            {
                return true;
            }
            return false;
        }

        public static RomInfo MergeRomInfo(IEnumerable<RomInfo> romInfos, RomInfo baseInfo = RomInfo.None)
        {
            foreach (var r in romInfos) 
            {
                baseInfo |= r;
            }
            return baseInfo;
        }
    }
}
