using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    /// <summary>
    /// Represents a scene number tag
    /// </summary>
    public sealed record SceneTag
        : RomTag
    {

        /// <summary>
        /// The type of the tag ("Scene")
        /// </summary>
        public override string Slug => "Scene";

        /// <summary>
        /// The scene number parsed from the tag.
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// The prefix of the scene number parsed from the tag.
        /// </summary>
        public string? Prefix { get; }

        /// <summary>
        /// A scene number as parsed in No-Intro names.
        /// </summary>
        /// <param name="number">The number parsed.</param>
        /// <param name="prefix">Any optional type or prefix, such as x, z, or xB</param>
        public SceneTag(string number, string? prefix) => (Number, Prefix, Category) = (number, prefix, TagCategory.None);
    }
}
