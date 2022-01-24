using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// A text tag indicating the publisher
    /// </summary>
    public sealed record PublisherTag
        : RomTag
    {
        /// <summary>
        /// The publisher name.
        /// </summary>
        public string Publisher { get; }


        /// <summary>
        /// The type of the tag ("Text")
        /// </summary>
        public override string Slug => "Publisher";

        /// <summary>
        /// A text tag without known semantic value
        /// </summary>
        /// <param name="publisher">The value of the tag.</param>
        /// <param name="category">The category of the tag.</param>
        internal PublisherTag(string publisher, TagCategory category) => (Publisher, Category) = (publisher, category);
    }
}
