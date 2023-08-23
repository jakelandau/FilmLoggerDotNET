using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace FilmLoggerDotNET
{
    public class Film
    {
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
