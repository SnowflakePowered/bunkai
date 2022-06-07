using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// A text tag without known semantic value, but potentially parser-dependent metatype semantics.
    /// </summary>
    public sealed record TextTag
        : RomTag
    {
        /// <summary>
        /// The text contained in the tag.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The metatatype of a tag that a parser may attach.
        /// </summary>
        public string? Metatype { get; }

        /// <summary>
        /// The type of the tag ("Text")
        /// </summary>
        public override string Slug => "Text";

        /// <summary>
        /// A text tag without known semantic value
        /// </summary>
        /// <param name="text">The value of the tag.</param>
        /// <param name="category">The category of the tag.</param>
        internal TextTag(string text, TagCategory category) => (Text, Category) = (text, category);

        /// <summary>
        /// A text tag with parser-dependent metatype semantic value.
        /// </summary>
        /// <param name="text">The value of the tag.</param>
        /// <param name="category">The category of the tag.</param>
        /// <param name="metatype">The metatype of the text tag.</param>
        internal TextTag(string text, string metatype, TagCategory category) => (Text, Metatype, Category) = (text, metatype, category);
    }
}
