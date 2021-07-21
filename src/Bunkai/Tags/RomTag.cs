using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// A ROM tag containing potentially semantic information about the ROM file name.
    /// 
    /// Parsed tags are lossy and do not contain enough information to reconstruct the
    /// filename. See https://docs.rs/shiratsu-naming/0.1.3/shiratsu_naming/naming/index.html for a
    /// fully lossless parser.
    /// </summary>
    public abstract record RomTag
    {
        /// <summary>
        /// The category of the tag.
        /// </summary>
        public TagCategory Category { get; init; }

        /// <summary>
        /// The name or type of the tag
        /// </summary>
        public abstract string Slug { get; }
    }
}
