using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    }
}
