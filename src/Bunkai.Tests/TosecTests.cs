using Bunkai.Parsers;
using Pidgin;
using System;
using Xunit;

namespace Bunkai.Tests
{
    public class TosecTests
    {
  
        [Theory]
        [InlineData("2600 Digital Clock - Demo 1 (demo)(1997-10-03)(Cracknell, Chris 'Crackers')(NTSC)(PD)", "2600 Digital Clock - Demo 1", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("Escape from the Mindmaster (1982)(Starpath)(PAL)(Part 3 of 4)[Supercharger Cassette]", "Escape from the Mindmaster", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("ZZZ-UNK-Show King Tut (1996) (Schick, Bastian) [a]", "Show King Tut", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("Cube CD 20, The (40) - Testing v1.203 (demo) (2020)(SomePublisher)", "The Cube CD 20 (40) - Testing", NamingConvention.TheOldSchoolEmulationCenter)]
        public void StructuredFilename_Tests(string filename, string title, NamingConvention convention)
        {
            var parser = new TosecParser();

            Assert.True(parser.TryParse(filename.Trim(), out var nameInfo));

            //var structuredFilename = new StructuredFilename(filename);
            //Assert.Equal(convention, structuredFilename.NamingConvention);
            //Assert.Equal(regioncode, nameInfo.Regio);
            Assert.Equal(title, nameInfo.NormalizedTitle);
        }
    }
}
