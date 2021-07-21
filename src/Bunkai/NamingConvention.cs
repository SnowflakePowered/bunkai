using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai
{
    /// <summary>
    /// Types of filename conventions
    /// </summary>
    public enum NamingConvention
    {
        /// <summary>
        /// Unknown naming convention, possibly invalid filename
        /// </summary>
        Unknown,

        /// <summary>
        /// No-Intro Naming Convention
        /// </summary>
        NoIntro,

        /// <summary>
        /// TOSEC Naming Convention
        /// </summary>
        TheOldSchoolEmulationCenter,

        /// <summary>
        /// GoodTools naming convention
        /// </summary>
        GoodTools,
    }
}
