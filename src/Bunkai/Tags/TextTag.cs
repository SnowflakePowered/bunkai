using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// A text tag without known semantic value
    /// </summary>
    public sealed record TextTag
        : RomTag
    {
        /// <summary>
        /// The text contained in the tag.
        /// </summary>
        public string Text { get; }


        /// <summary>
        /// The type of the tag ("Text")
        /// </summary>
        public override string Slug => "Text";

        /// <summary>
        /// A text tag without known semantic value
        /// </summary>
        /// <param name="text">The value of the tag.</param>
        /// <param name="category">The category of the tag.</param>
        public TextTag(string text, TagCategory category) => (Text, Category) = (text, category);
    }
}
