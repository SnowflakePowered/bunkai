using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    public sealed record MediaTag
        : RomTag
    {
        internal MediaTag(string mediaType, string number, string? total, string subpart) => (MediaType, Part, Total, SubPart, Category) = (mediaType, number, total, subpart, TagCategory.Parenthesized);
        internal MediaTag(string mediaType, string number, string? total) => (MediaType, Part, Total, Category) = (mediaType, number, total, TagCategory.Parenthesized);
        internal MediaTag(string mediaType, string number) => (MediaType, Part, Category) = (mediaType, number, TagCategory.Parenthesized);

        /// <summary>
        /// The type of the media, i.e. 'Disc' or 'Cassette'
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        /// The part number of the media.
        /// </summary>
        public string Part { get; }

        /// <summary>
        /// The part number of the media.
        /// </summary>
        public string? SubPart { get; }

        /// <summary>
        /// The total number of parts of the media in the full collection.
        /// </summary>
        public string? Total { get; }

        public override string Slug => "Media";
    }
}
