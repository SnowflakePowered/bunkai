using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkai
{
    /// <summary>
    /// Info flags associated with the filename
    /// </summary>
    [Flags]
    public enum RomInfo : long
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,

        /// <summary>
        /// The game dump is unlicensed
        /// </summary>
        Unlicensed = 1 << 0,

        /// <summary>
        /// The game dump is of a demo.
        /// 
        /// Corresponds to any TOSEC 'demo' flag, as well as the NoIntro Demo and 'Taikenban' flag.
        /// </summary>
        Demo = 1 << 1,

        /// <summary>
        /// The original software has been deliberately hacked/altered to remove some form of copy protection.
        /// </summary>
        Cracked = 1 << 2,

        /// <summary>
        /// The original software has been deliberately hacked/altered in some way to 'improve' or fix the image to work in a non-standard way,
        /// </summary>
        Fixed = 1 << 3,

        /// <summary>
        /// The original software has been deliberately hacked/altered in some way, such as adding an intro or changing in game sprites or text.
        /// </summary>
        Hacked = 1 << 4,

        /// <summary>
        /// The original software has been hacked/altered in some way (but not deliberately), 
        /// e.g. if you dumped an original UNTOUCHED floppy disk (say it is a game for some microcomputer), 
        /// the image would also be original/clean. If the floppy disk had been played/loaded (BUT NOT WRITE PROTECTED), 
        /// then the disk might have an additional file saved back to it such as a saved game, or saved high score table. 
        /// If you then re-dumped it, the image would no longer be original/clean, and a [m] flag would be appropriate.
        /// </summary>
        Modified = 1 << 5,

        /// <summary>
        /// The software is not legally licensed or violates some international IP.
        /// </summary>
        Pirated = 1 << 6,

        /// <summary>
        /// The original software has been deliberately hacked/altered to add cheats and/or a cheat menu.
        /// </summary>
        Trained = 1 << 7,

        /// <summary>
        /// The original software has been deliberately hacked/altered to translate into a different language than originally published/released.
        /// </summary>
        Translated = 1 << 8,

        /// <summary>
        /// The image is damaged (duplicated data or too much data).
        /// </summary>
        OverDump = 1 << 9,

        /// <summary>
        /// The image is damaged (missing data).
        /// </summary>
        UnderDump = 1 << 10,

        /// <summary>
        /// The image is damaged. This is a general 'damaged/bad' flag, to be used when the type of
        /// damage does not fit into any of the other 'damaged' categories. 
        /// It is likely this image will not work properly, or not at all.
        /// </summary>
        BadDump = 1 << 11,

        /// <summary>
        /// Image has had multiply person/multi dump verification to confirm it is a 100% repeatable and correct dump.
        /// </summary>
        GoodDump = 1 << 12,

        /// <summary>
        /// The image is damaged from the infection of a virus.
        /// </summary>
        Virus = 1 << 13,

        /// <summary>
        /// An alternate ORIGINAL version of another ORIGINAL image, e.g. if a game was released, 
        /// then re-released later with a small change (and the revision/version number is not known).
        /// </summary>
        Alternate = 1 << 14,

        /// <summary>
        /// Early test build.
        /// </summary>
        Alpha = 1 << 15,

        /// <summary>
        /// Later, feature complete test build.
        /// </summary>
        Beta = 1 << 16,
        /// <summary>
        /// Near complete build.
        /// </summary>
        Preview = 1 << 17,
        /// <summary>
        /// Near complete build.
        /// </summary>
        Prerelease = 1 << 18,

        /// <summary>
        /// Unreleased, prototype software.
        /// </summary>
        Prototype = 1 << 19,

        /// <summary>
        /// Development software
        /// </summary>
        Development = Alpha | Beta | Preview | Prerelease,

        /// <summary>
        /// Kiosk
        /// Also applicable to 'Tentou Taikenban'
        /// </summary>
        Kiosk = 1 << 20,

        /// <summary>
        /// Sample 
        /// </summary>
        Sample = 1 << 21,
        
        /// <summary>
        /// Bonus Disc
        /// </summary>
        Bonus = 1 << 22,

        /// <summary>
        /// Either a demo, sample, kiosk version, or a bonus
        /// </summary>
        EvaluationVersion = Demo | Sample | Kiosk,

        /// <summary>
        /// A BIOS file.
        /// </summary>
        BIOS = 1 << 23
    }
}
