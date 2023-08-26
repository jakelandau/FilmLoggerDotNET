using System.Text.Json;

namespace FilmLoggerDotNET.Tests
{

	public class BusinessLogicTests
	{

		[Theory]
		[InlineData("tt9362722", "https://image.tmdb.org/t/p/w500/8Vt6mWEReuy4Of61Lnj5Xj704m8.jpg")] // Spider-Man: Across the Spider-Verse (2023)
		[InlineData("tt0050083", "https://image.tmdb.org/t/p/w500/ow3wq89wM8qd5X7hWKxiRfsFf9C.jpg")] // 12 Angry Men (1957)
		[InlineData("tt0167260", "https://image.tmdb.org/t/p/w500/rCzpDGLbOoPwLjy3OAm5NUPOTrC.jpg")] // LOTR: The Return of the King (2003)
		[InlineData("tt0109830", "https://image.tmdb.org/t/p/w500/arw2vcBveWOVZr6pxd9XTd1TdQa.jpg")] // Forrest Gump (1994)

		public async Task VerifyFilmPositiveTest(string searchKey, string correctResponse)
        {
			// Instances Test Object for BusinessLogic
			var testLogicProcessor = new BusinessLogic();

			// Deserializes secret.json to obtain API keys
			var secretPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "FilmLoggerDotNET", "secret.json");
			var APIKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(json: File.ReadAllText(secretPath));

			// Attempts to instance client using API Key
			testLogicProcessor.APIKey = APIKeys!["TMDbAPI"];
			testLogicProcessor.CreateWebClient();

			var posterPath = await testLogicProcessor.VerifyFilmAsync(searchKey);
			Assert.Equal(correctResponse, posterPath);
        }
	}
}