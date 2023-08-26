using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace FilmLoggerDotNET
{
    public class Film
    {
		// Order indicates the order of every film within the larger list
		// handling the edge case for when multiple films are seen on the
		// same YYYY-MM-DD. 
		public int Order { get; set; }

		// The rest of these are nullable only because they aren't all
		// implemented at the same time in the business logic, so it
		// would throw an error otherwise.
        public int? Day { get; set; }
        public string? ImdbId { get; set; }
        public int? Month { get; set; }
        public bool? Theater { get; set; }
        public string? Title { get; set; }
        public int? Year { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(List<Film>))]
    internal partial class FilmSerializerContext : JsonSerializerContext
    {

    }
}
