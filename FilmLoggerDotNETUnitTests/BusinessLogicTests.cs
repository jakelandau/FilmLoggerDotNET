using System.Text.Json;

namespace FilmLoggerDotNET.Tests
{

	public class BusinessLogicTests
	{
		// Deserializes secret.json to obtain API keys.
		// You must run FilmLoggerDotNET normally, or manually create the secret.json
		// and put it in the ApplicationData folder yourself, or the unit tests will
		// fail. The Dictionary Key is "TMDbAPI" and the Value is your own API Key.
		private static readonly string secretPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "FilmLoggerDotNET", "secret.json");
		private Dictionary<string,string> APIKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(json: File.ReadAllText(path: secretPath))!;

		[Theory]
		[InlineData("tt0050083", "https://image.tmdb.org/t/p/w500/ow3wq89wM8qd5X7hWKxiRfsFf9C.jpg")] // 12 Angry Men (1957)
		[InlineData("tt0133093", "https://image.tmdb.org/t/p/w500/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg")] // The Matrix (1999)
		[InlineData("tt0047478", "https://image.tmdb.org/t/p/w500/iAq0sq42vKTLneVGqHn1D4GzgrM.jpg")] // Seven Samurai (1954)
		[InlineData("tt0245429", "https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg")] // Spirited Away (2002)
		[InlineData("tt0095327", "https://image.tmdb.org/t/p/w500/k9tv1rXZbOhH7eiCk378x61kNQ1.jpg")] // Grave of the Fireflies (1988)
		public async Task VerifyFilmPositiveTest(string searchKey, string correctResponse)
        {
			var testLogicProcessor = new BusinessLogic
			{
				APIKey = APIKeys!["TMDbAPI"]
			};
			testLogicProcessor.CreateWebClient();

			var posterPath = await testLogicProcessor.VerifyFilmAsync(searchKey);
			Assert.Equal(correctResponse, posterPath);
        }

		[Theory]
		[InlineData("tt9365522")] // Bairi Piya Episode #1.105
		[InlineData("tt0000")] // 404 Error
		[InlineData("tt0903747")] // Breaking Bad (2008-2013)
		public async Task VerifyFilmNegativeTest(string searchKey)
		{
			var testLogicProcessor = new BusinessLogic
			{
				APIKey = APIKeys!["TMDbAPI"]
			};
			testLogicProcessor.CreateWebClient();

			await Assert.ThrowsAsync<NullReferenceException>(async () => await testLogicProcessor.VerifyFilmAsync(searchKey));
		}

		[Fact]
		public async Task VerifyWorkingArchiveSort()
		{

			var testLogicProcessor = new BusinessLogic
			{
				APIKey = APIKeys!["TMDbAPI"]
			};
			testLogicProcessor.CreateWebClient();

			// Creates two films on date 2014-11-20
			foreach (var imdbID in new List<string> { "tt0167260", "tt0088763" }) // LOTR: Return of the King (2003), then Back To The Future (1985)
			{
				await testLogicProcessor.VerifyFilmAsync(imdbID);
				testLogicProcessor.AddFilmToArchive(true, 20, 11, 2014);
			}

			// Creates one film on date 2014-12-03
			await testLogicProcessor.VerifyFilmAsync("tt1375666"); // Inception (2010)
			testLogicProcessor.AddFilmToArchive(true, 3, 12, 2014);

			// Creates one film between those dates on 2014-11-25
			await testLogicProcessor.VerifyFilmAsync("tt2582802"); // Whiplash (2014)
			testLogicProcessor.AddFilmToArchive(true, 25, 11, 2014);

			// Verifies that the film added fourth is sorted to the third index
			// based on it's date relative to the other film's watchdates.
			Assert.Equal(testLogicProcessor.WorkingMovieArchive()[2].Day, 25);
		}
	}
}