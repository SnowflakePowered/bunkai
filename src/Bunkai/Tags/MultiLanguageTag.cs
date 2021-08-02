using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// Represents a multilanguage tag without only the count of languages specified
    /// </summary>
    public sealed record MultiLanguageTag
        : RomTag
    {

        public MultiLanguageTag(int languageCount) => (Count, Category) = (languageCount, TagCategory.Parenthesized);

        public override string Slug => "MultiLanguage";

        public int Count { get; }
    }
}
