using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// Represents a version in a filename
    /// </summary>
    public sealed record VersionTag
        : RomTag
    {
        /// <summary>
        /// The tag used to specify this version. 
        /// For example, 'Version' or 'Rev'.
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// The major version or revision number.
        /// </summary>
        public string Major { get; }

        /// <summary>
        /// The minor version or revision number
        /// </summary>
        public string? Minor { get; }

        /// <summary>
        /// The version prefix, if any.
        /// This is not the version type, but the prefix that appears, separated by a space, before the type.
        /// </summary>
        public string? Prefix { get; }

        /// <summary>
        /// The version suffix, if any.
        /// This appears after the version number, separated by a space.
        /// </summary>
        public ImmutableArray<string> Suffixes { get; } = ImmutableArray<string>.Empty;

        /// <summary>
        /// The type of the tag ("Version")
        /// </summary>
        public override string Slug => "Version";

        /// <summary>
        /// Intantiates a Version instance.
        /// </summary>
        /// <param name="tag">The tag used to specify this version. </param>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="tagType">The type of the tag.</param>
        internal VersionTag(string tag, string major, string minor, TagCategory tagType) => (Tag, Major, Minor, Category) = (tag, major, minor, tagType);

        /// <summary>
        /// Intantiates a Version instance without a minor version.
        /// </summary>
        /// <param name="tag">The tag used to specify this version. </param>
        /// <param name="major">The major version.</param>
        internal VersionTag(string tag, string major, TagCategory tagType) => (Tag, Major, Minor, Category) = (tag, major, null, tagType);

        /// <summary>
        /// Intantiates a Version instance.
        /// </summary>
        /// <param name="tag">The tag used to specify this version. </param>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="tagType">The type of the tag.</param>
        internal VersionTag(string tag, string major, string? minor, string? suffix, TagCategory tagType) => (Tag, Major, Minor, Suffixes, Category) = (tag, major, minor, suffix != null ? ImmutableArray.Create(suffix) : ImmutableArray<string>.Empty, tagType);

        /// <summary>
        /// Intantiates a Version instance.
        /// </summary>
        /// <param name="tag">The tag used to specify this version. </param>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="tagType">The type of the tag.</param>
        internal VersionTag(string tag, string major, string? minor, IEnumerable<string> suffixes, TagCategory tagType) => (Tag, Major, Minor, Suffixes, Category) = (tag, major, minor, suffixes.ToImmutableArray(), tagType);

    }
}
