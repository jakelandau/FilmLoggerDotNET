using System;

namespace FilmLoggerDotNET
{
    public class Film
    {
        public int? day { get; set; }
        public string? imdb_id { get; set; }
        public int? month { get; set; }
        public bool? theater { get; set; }
        public string? title { get; set; }
        public int? year { get; set; }

        public string summary()
        {
            return $"{title} - {year}-{month}-{day} - In Theater: {theater} - {imdb_id}";
        }
    }
}
