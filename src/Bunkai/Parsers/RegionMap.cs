using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Parsers
{
    internal static class RegionMap
    {
        public readonly static ImmutableDictionary<string, Region> NOINTRO_MAP = new Dictionary<string, Region>() {
            { "Australia" , Region.Australia},
            { "Argentina" , Region.Argentina},
            { "Brazil" , Region.Brazil},
            { "Canada" , Region.Canada},
            { "China" , Region.China},
            { "Denmark" , Region.Denmark},
            { "Netherlands" , Region.Netherlands},
            { "Europe" , Region.Europe},
            { "France" , Region.France},
            { "Germany" , Region.Germany},
            { "Greece" , Region.Greece},
            { "Hong Kong" , Region.HongKong},
            { "Italy" , Region.Italy},
            { "Japan" , Region.Japan},
            { "Korea" , Region.SouthKorea},
            { "Norway" , Region.Norway},
            { "Russia" , Region.Russia},
            { "Spain" , Region.Spain},
            { "Sweden" , Region.Sweden},
            { "USA" , Region.UnitedStates},
            { "UK" , Region.UnitedKingdom},
            { "United Kingdom" , Region.UnitedKingdom},
            { "Asia" , Region.Asia},
            { "Poland" , Region.Poland},
            { "Portugal" , Region.Portugal},
            { "Ireland" , Region.Ireland},
            { "Unknown" , Region.Unknown},
            { "Taiwan" , Region.Taiwan},
            { "Finland" , Region.Finland},
            { "UAE" , Region.UnitedArabEmirates},
            { "Albania" , Region.Albania},
            { "Austria" , Region.Austria},
            { "Bosnia" , Region.Bosnia},
            { "Belgium" , Region.Belgium},
            { "Bulgaria" , Region.Bulgaria},
            { "Switzerland" , Region.Switzerland},
            { "Chile" , Region.Chile},
            { "Serbia" , Region.Serbia},
            { "Cyprus" , Region.Cyprus},
            { "Czech Republic" , Region.Czechia},
            { "Czechia" , Region.Czechia},
            { "Estonia" , Region.Estonia},
            { "Egypt" , Region.Egypt},
            { "Croatia" , Region.Croatia},
            { "Hungary" , Region.Hungary},
            { "Indonesia" , Region.Indonesia},
            { "Israel" , Region.Israel},
            { "India" , Region.India},
            { "Iran" , Region.Iran},
            { "Iceland" , Region.Iceland},
            { "Jordan" , Region.Jordan},
            { "Lithuania" , Region.Lithuania},
            { "Luxembourg" , Region.Luxembourg},
            { "Latvia" , Region.Latvia},
            { "Mongolia" , Region.Mongolia},
            { "Mexico" , Region.Mexico},
            { "Malaysia" , Region.Malaysia},
            { "Nepal" , Region.Nepal},
            { "New Zealand" , Region.NewZealand},
            { "Oman" , Region.Oman},
            { "Peru" , Region.Peru},
            { "Philippines" , Region.Philippines},
            { "Qatar" , Region.Qatar},
            { "Romania" , Region.Romania},
            { "Singapore" , Region.Singapore},
            { "Slovenia" , Region.Slovenia},
            { "Slovakia" , Region.Slovakia},
            { "Thailand" , Region.Thailand},
            { "Turkey" , Region.Turkey},
            { "Vietnam" , Region.Vietnam},
            { "Yugoslavia" , Region.Yugoslavia},
            { "South Africa" , Region.SouthAfrica},
            { "The Netherlands" , Region.Netherlands},
            }.ToImmutableDictionary();


        public static readonly ImmutableArray<Region> WORLD = ImmutableArray.Create(Region.UnitedStates, Region.Japan, Region.Europe);
        public static readonly ImmutableArray<Region> SCANDINAVIA = ImmutableArray.Create(Region.Denmark, Region.Norway, Region.Sweden);
        public static readonly ImmutableArray<Region> LATIN_AMERICA = ImmutableArray.Create(Region.Mexico, Region.Brazil, Region.Argentina, Region.Chile, Region.Peru);
    }
}
