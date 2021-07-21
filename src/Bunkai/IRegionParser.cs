using System.Collections.Immutable;

namespace Bunkai
{
    /// <summary>
    /// Implements a parser that returns a list of regions from a region string
    /// </summary>
    public interface IRegionParser
    {
        /// <summary>
        /// Returns an array of <see cref="Region"/> parsed from the region string. If the region string is invalid, 
        /// returns an empty array. 
        /// </summary>
        /// <param name="regionString">The input string.</param>
        /// <returns>An immutable array of <see cref="Region"/> contained in the region string.</returns>
        ImmutableArray<Region> ParseRegion(string regionString);
    }
}
