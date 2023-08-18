using Avalonia.Controls;
using FilmLoggerDotNET;

namespace FilmLoggerDotNETUnitTests
{
    public class LogicTests
    {
        [Theory]
        [InlineData("tt9362722")] // Spider-Man: Across the Spider-Verse
        [InlineData("tt9362552")] // Daldalita: Episode #1.1
        [InlineData("tt936552")]  // 404 Error
        public void VerifyFilmTest(string IMDbID)
        {

        }
    }
}