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
        [InlineData("ZZZ-UNK-Befok#Packraw (20021012) by Jum Hig (PD)", "Befok#Packraw", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("ZZZ-UNK-Micro Font Dumper by Schick, Bastian (199x) (PD)", "Micro Font Dumper", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("Escape from the Mindmaster (1982)(Starpath)(PAL)(Part 3 of 4)[Supercharger Cassette]", "Escape from the Mindmaster", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("ZZZ-UNK-Show King Tut (1996) (Schick, Bastian) [a]", "Show King Tut", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("Cube CD 20, The (40) - Testing v1.203 (demo) (2020)(SomePublisher)", "The Cube CD 20 (40) - Testing", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("Motocross & Pole Position (Starsoft - JVP)(PAL)[b1][possible unknown mode]", "Motocross & Pole Position", NamingConvention.TheOldSchoolEmulationCenter)]
        [InlineData("ZZZ-UNK-Tron 6 fun v0.15", "Tron 6 fun", NamingConvention.TheOldSchoolEmulationCenter)]
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
