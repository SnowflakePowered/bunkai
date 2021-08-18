using System.Collections.Immutable;

namespace Bunkai.Tags
{
    /// <summary>
    /// Represents a tag with one or more specified regions
    /// </summary>
    public sealed record RegionTag
        : RomTag
    {
        /// <summary>
        /// The regions specified in the tag.
        /// </summary>
        public ImmutableArray<Region> Regions { get; }

        public override string Slug => "Region";

        internal RegionTag(params Region[] regions) => (Regions, Category) = (regions.ToImmutableArray(), TagCategory.Parenthesized);
    }
}
