using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// Represents a tag with one or more specified languages.
    /// </summary>
    public sealed record LanguageTag
        : RomTag
    {
        /// <summary>
        /// The languages specified in the tag.
        /// </summary>
        public ImmutableArray<Language> Languages { get; }

        public override string Slug => "Language";

        internal LanguageTag(params Language[] languages) => (Languages, Category) = (languages.ToImmutableArray(), TagCategory.Parenthesized);
    }
}
