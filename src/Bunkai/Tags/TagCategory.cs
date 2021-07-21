using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// Represents the type of tag.
    /// </summary>
    public enum TagCategory
    {
        /// <summary>
        /// The tag is neither parenthesized or bracketed.
        /// </summary>
        None,
        /// <summary>
        /// The tag is parenthesized.
        /// </summary>
        Parenthesized,
        /// <summary>
        /// The tag is bracketed.
        /// </summary>
        Bracketed
    }
}
