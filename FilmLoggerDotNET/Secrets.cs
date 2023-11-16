using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace FilmLoggerDotNET
{
    public class Secrets
    {
		// Stores TMDb API key 
		public string? TMDbAPI { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Secrets))]
    internal partial class SecretsSerializerContext : JsonSerializerContext
    {

    }
}
