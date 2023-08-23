using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using FilmLoggerDotNET;
using FilmLoggerDotNETUnitTests;

[assembly: AvaloniaTestApplication(typeof(LogicTests))]
namespace FilmLoggerDotNETUnitTests
{
	
    public class LogicTests
    {
		public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UseHeadless(new AvaloniaHeadlessPlatformOptions());

		[AvaloniaFact]
        public void VerifyFilmTest()
        {

        }
    }
}