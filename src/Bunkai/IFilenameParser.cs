using System.Diagnostics.CodeAnalysis;

namespace Bunkai
{
    /// <summary>
    /// A parser for a structured filename.
    /// </summary>
    public interface IFilenameParser
    {
        /// <summary>
        /// Tries to parse a name info.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="nameInfo"></param>
        /// <returns></returns>
        public bool TryParse(string filename, [NotNullWhen(true)] out NameInfo? nameInfo);
    }
}
