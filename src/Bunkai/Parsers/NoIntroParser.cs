using System;
using System.Collections.Generic;
using System.Linq;
using Pidgin;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Bunkai.Tags;

namespace Bunkai.Parsers
{
    public sealed partial class NoIntroParser
        : IFilenameParser, IRegionParser
    {
        private static ImmutableArray<Region> ParseRegion(string regionString)
        {
            var regions = new HashSet<Region>();

            foreach (string regionCode in regionString.Split(", "))
            {
                if (!regionCode.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                    return ImmutableArray.Create<Region>();
                string region = regionCode.Trim();
                ImmutableArray<Region> rs = region switch
                {
                    "World" => RegionMap.WORLD,
                    "World (guessed)" => RegionMap.WORLD,
                    "World (Guessed)" => RegionMap.WORLD,
                    "Scandinavia" => RegionMap.SCANDINAVIA,
                    "Latin America" => RegionMap.LATIN_AMERICA,
                    _ => RegionMap.NOINTRO_MAP.ContainsKey(region) ? ImmutableArray.Create(RegionMap.NOINTRO_MAP[region]) : ImmutableArray.Create<Region>(),
                };

                if (rs.Length == 0)
                {
                    // Invalid region string
                    return ImmutableArray.Create<Region>();
                }

                foreach (Region r in rs)
                {
                    regions.Add(r);
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

        private static ImmutableArray<RomTag> MergeTags(IEnumerable<IEnumerable<RomTag>> flags2, params Maybe<RomTag>[] flags)
        {
            return flags.Where(h => h.HasValue).Select(h => h.Value).Concat(flags2.SelectMany(f => f)).ToImmutableList().ToImmutableArray();
        }

        private static RomInfo MergeInfoFlags(ImmutableArray<RomTag> flags)
        {
            RomInfo flag = RomInfo.None;
            
            foreach (var maybeFlag in flags)
            {
                flag |= maybeFlag switch
                {
                    ReleaseTag { Status: "Proto" or "Prototype" } => RomInfo.Prototype,
                    ReleaseTag { Status: "Kiosk" } => RomInfo.Kiosk,
                    ReleaseTag { Status: "Demo" } => RomInfo.Demo,
                    ReleaseTag { Status: "Beta" } => RomInfo.Beta,
                    ReleaseTag { Status: "Sample" } => RomInfo.Sample,
                    TextTag { Text: "Bonus Disc", Category: TagCategory.Parenthesized } => RomInfo.Bonus,
                    TextTag { Text: "Unl", Category: TagCategory.Parenthesized } => RomInfo.Unlicensed,
                    TextTag { Text: "b", Category: TagCategory.Bracketed } => RomInfo.BadDump,
                    TextTag { Text: "BIOS", Category: TagCategory.Bracketed } => RomInfo.BIOS,
                    _ => RomInfo.None,
                };
            }
            return flag;
        }

    }
}
