using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    public sealed record ReleaseTag
        : RomTag
    {
      
        /// <summary>
        /// The status of the release, such as Demo, Beta, Kiosk, etc.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Secondary information such as the number or state of the demo.
        /// </summary>
        public string? Meta { get; }


        /// <summary>
        /// The type of the tag ("Release")
        /// </summary>
        public override string Slug => "Release";

        /// <summary>
        /// A release tag as seen in NoIntro or TOSEC names
        /// </summary>
        /// <param name="status">The status of the release.</param>
        /// <param name="meta">Meta information associated with the release.</param>
        public ReleaseTag(string status, string? meta) => (Status, Meta) = (status, meta);
    }
}
