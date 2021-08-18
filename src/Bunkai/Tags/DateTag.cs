using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai.Tags
{
    public sealed record DateTag
       : RomTag
    {
        public override string Slug => "Date";

        public string? Year { get; }
        public string? Month { get; }
        public string? Day { get; }

        internal DateTag(string? year, string? month, string? day) => (Year, Month, Day, Category) = (year, month, day, TagCategory.Parenthesized);
    }
}
