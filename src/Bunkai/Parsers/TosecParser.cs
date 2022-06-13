using Bunkai.Tags;
using Pidgin;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Parsers
{
    public sealed partial class TosecParser
        : IFilenameParser, IRegionParser
    {
        private static ImmutableArray<Region> ParseRegion(string regionString)
        {
            var regions = new HashSet<Region>();

            foreach (string regionCode in regionString.Split("-"))
            {
                if (RegionMap.TOSEC_MAP.TryGetValue(regionCode, out Region region))
                {
                    regions.Add(region);
                } 
                else
                {
                    // bad region code.
                    return ImmutableArray.Create<Region>();
                }
            }

            return regions.ToImmutableArray();
        }

        ImmutableArray<Region> IRegionParser.ParseRegion(string regionString) => ParseRegion(regionString);

        public bool TryParse(string filename, [NotNullWhen(true)] out NameInfo? nameInfo)
        {
            var res = NameParser.Parse(filename);
            nameInfo = res.Success ? res.Value : null;
            return res.Success;
        }

        private static RomInfo CombineRomInfo(IEnumerable<RomTag> romTags, RomInfo dumpBase)
        {
            foreach (var maybeFlag in romTags)
            {
                dumpBase |= maybeFlag switch
                {
                    ReleaseTag { Meta: "kiosk" } => RomInfo.Kiosk,
                    ReleaseTag { Status: "demo" } => RomInfo.Demo,
                    ReleaseTag { Status: "Proto" or "Prototype" or "proto" } => RomInfo.Prototype,
                    ReleaseTag { Status: "Preview" or "preview" } => RomInfo.Preview,
                    ReleaseTag { Status: "Pre-Release" or "pre-release" } => RomInfo.Prerelease,
                    ReleaseTag { Status: "Alpha" or "alpha" } => RomInfo.Alpha,
                    ReleaseTag { Status: "Beta" or "beta" } => RomInfo.Beta,
                    _ => RomInfo.None,
                };
            }
            return dumpBase;
        }

        private static RomInfo MatchDumpFlag(string tag)
        {
            return tag switch
            {
                "cr" => RomInfo.Cracked,
                "f" => RomInfo.Fixed,
                "h" => RomInfo.Hacked,
                "m" => RomInfo.Modified,
                "p" => RomInfo.Pirated,
                "t" => RomInfo.Trained,
                "tr" => RomInfo.Translated,
                "o" => RomInfo.OverDump,
                "u" => RomInfo.UnderDump,
                "v" => RomInfo.Virus,
                "b" => RomInfo.BadDump,
                "a" => RomInfo.Alternate,
                _ => RomInfo.None
            };
        }
    }
}
