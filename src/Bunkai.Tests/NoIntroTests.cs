using Bunkai.Parsers;
using Pidgin;
using System;
using Xunit;

namespace Bunkai.Tests
{
    public class NoIntroTests
    {
  
        [Fact]
        public void TryParse_Test()
        {
            var parser = new NoIntroParser();
            //Assert.True(parser.TryParse("FIFA 20 - Portuguese (Brazil) In-Game Commentary (World)", out var fifa20));
            //Assert.True(parser.TryParse("Odekake Lester - Lelele no Le (^^; (Japan)", out var odekake));
            //Assert.True(parser.TryParse("void tRrLM(); Void Terrarium (Japan)", out var voidTer));
            //Assert.True(parser.TryParse("xB14 - [BIOS] void tRrLM(); Void Terrarium (Japan) (Multi Tap (SCPH-10090) Doukonban) (Tag)", out var voidTerx));
            //Assert.True(parser.TryParse("xB14 - [BIOS] void tRrLM(); Void Terrarium (Japan) (Multi Tap (SCPH-10090) Doukonban) (Tag) (Beta 2) (Proto 2)", out var voidTerxx));
            Assert.True(parser.TryParse("Pachio-kun 3 (Japan) (Rev A)", out var rev));
            Assert.True(parser.TryParse("Pachio-kun 3 (Japan) (v1.x)", out rev));
            Assert.True(parser.TryParse("Pachio-kun 3 (Japan) (Version 10.5.1-56 A Alt, v124)", out rev));
        }

        [Fact]
        public void TryParse2_Test()
        {
            var parser = new NoIntroParser();


            var res = NoIntroParser.ParseLanguageTag.Parse("(En)");
            //Assert.True(parser.TryParse("Prince of Persia (Europe) ", out var voidTerx));

        }
        
        [Theory]
        [InlineData("Super Mario Bros. 2 (USA)", "Super Mario Bros. 2", NamingConvention.NoIntro, "US")]
        [InlineData("Super Mario Bros. (USA)", "Super Mario Bros.", NamingConvention.NoIntro, "US")]
        [InlineData("RPG Maker Fes (Europe) (En,Fr,De,Es,It)", "RPG Maker Fes", NamingConvention.NoIntro, "EU")]
        [InlineData("Seisen Chronicle (Japan) (eShop) [b]", "Seisen Chronicle", NamingConvention.NoIntro, "JP")]
        [InlineData("Pachio-kun 3 (Japan) (Rev A)", "Pachio-kun 3", NamingConvention.NoIntro, "JP")]
        [InlineData("Barbie - Jet, Set & Style! (Europe) (De,Es,It) (VMZX) (NDSi Enhanced)",
           "Barbie - Jet, Set & Style!", NamingConvention.NoIntro, "EU")]
        [InlineData(
           "Barbie - Jet, Set & Style! (Europe) (De,Es,It) (Beta) (Proto) (Sample) (Unl) (VMZX) (NDSi Enhanced)",
           "Barbie - Jet, Set & Style!", NamingConvention.NoIntro, "EU")]
        [InlineData("Bart Simpson's Escape from Camp Deadly (USA, Europe)", "Bart Simpson's Escape from Camp Deadly",
           NamingConvention.NoIntro, "US-EU")]
        [InlineData("[BIOS] X'Eye (USA) (v2.00)", "X'Eye", NamingConvention.NoIntro, "US")]
        [InlineData("[BIOS] X'Eye (USA) (v2.00) (En,Es,Fr)", "X'Eye", NamingConvention.NoIntro, "US")]
        [InlineData("Adventures of Batman & Robin, The (USA)", "The Adventures of Batman & Robin",
           NamingConvention.NoIntro, "US")]
        [InlineData("Pokemon - Versione Blu (Italy) (SGB Enhanced)", "Pokemon - Versione Blu",
           NamingConvention.NoIntro, "IT")]
        [InlineData("Pop'n TwinBee (Europe)", "Pop'n TwinBee", NamingConvention.NoIntro, "EU")]
        [InlineData("Prince of Persia (Europe) (En,Fr,De,Es,It)", "Prince of Persia", NamingConvention.NoIntro,
           "EU")]
        [InlineData("Purikura Pocket 2 - Kareshi Kaizou Daisakusen (Japan) (SGB Enhanced)",
           "Purikura Pocket 2 - Kareshi Kaizou Daisakusen",
           NamingConvention.NoIntro, "JP")]
        [InlineData("Legend of Zelda, The - A Link to the Past (Canada)",
           "The Legend of Zelda - A Link to the Past", NamingConvention.NoIntro, "CA")]
        public void StructuredFilename_Tests(string filename, string title, NamingConvention convention,
           string regioncode)
        {
            var parser = new NoIntroParser();

            Assert.True(parser.TryParse(filename.Trim(), out var nameInfo));

            //var structuredFilename = new StructuredFilename(filename);
            //Assert.Equal(convention, structuredFilename.NamingConvention);
            //Assert.Equal(regioncode, nameInfo.Regio);
            Assert.Equal(title, nameInfo.NormalizedTitle);
        }
    }
}
