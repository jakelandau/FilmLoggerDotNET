using Avalonia.Platform.Storage;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;

namespace FilmLoggerDotNET
{
	public class BusinessLogic
	{
		private List<Film> workingMovieArchive = new List<Film>(); // Working archive of all movies user has watched
		private List<Film> safetyCheckMovieArchive = new List<Film>(); // Copy of above made for safety check; if working archive doesn't match safety check, data has not been saved

		private TMDbClient webClient; // Client for accessing TheMovieDB API
		public string APIKey { get; set; } = "";

		private Film currentMovie = new Film(); // Film object in buffer to add to working archive
		private bool isVerified = false; // Latch to ensure IMDb ID has been verified before attempting to add to archive

		public bool IsVerified() { return isVerified; }

		public int GetFilmCount() { return workingMovieArchive.Count; }
		public int GetSafetyCount() { return safetyCheckMovieArchive.Count; }

		public void CreateWebClient()
		{
			// Attempts to instance client using API Key
			webClient = new TMDbClient(APIKey);
		}

		public async Task ReadArchiveFileAsync(Stream stream)
		{
			try
			{
				// Deserializes provided stream into the workingMovieArchive
				using var streamReader = new StreamReader(stream);
				var JSONString = await streamReader.ReadToEndAsync();
				workingMovieArchive = JsonSerializer.Deserialize(json: JSONString, jsonTypeInfo: FilmSerializerContext.Default.ListFilm)!;

				// Copies working archive for dump safety checks
				safetyCheckMovieArchive = new List<Film>(workingMovieArchive!);
			}
			catch (JsonException ex)
			{
				throw ex;
			}
		}

		public async Task SaveArchiveFileAsync(IStorageFile fileDestination)
		{
			// Sorts workingMovieArchive based on Order index
			workingMovieArchive = workingMovieArchive.OrderBy(x => x.Order).ToList();

			// Writes on async worker thread to file path
			await using var stream = await fileDestination.OpenWriteAsync();
			using var streamWriter = new StreamWriter(stream);
			await streamWriter.WriteLineAsync(JsonSerializer.Serialize(workingMovieArchive, jsonTypeInfo: FilmSerializerContext.Default.ListFilm));

			// Clears working archive and safety copy
			EraseWorkingArchive();
		}

		public async Task<string> VerifyFilmAsync(string IMDbSearchKey)
		{
			try
			{
				// Attempts to verify film with TheMovieDB backend,
				// if successful populates initial data into currentMovie buffer
				// and releases verification latch
				Movie webSourcedFilm = await webClient.GetMovieAsync(imdbId: IMDbSearchKey);
				currentMovie = new Film
				{
					Order = workingMovieArchive.Count + 1,
					ImdbId = webSourcedFilm.ImdbId,
					Title = webSourcedFilm.Title
				};
				isVerified = true;

				// Returns poster image path for the TMDb link provided by the API

				return $"https://image.tmdb.org/t/p/w500{webSourcedFilm.PosterPath}";
			}
			catch (Exception)
			{
				// Clears buffers and re-sets verification latch
				currentMovie = new Film();
				isVerified = false;

				throw;
			}
		}

		public void AddFilmToArchive(bool inTheater, int Day, int Month, int Year)
		{
			currentMovie.Theater = inTheater;
			currentMovie.Day = Day;
			currentMovie.Month = Month;
			currentMovie.Year = Year;

			// Ensures that Film is placed into proper Order index based on the date
			// assuming that if another film was already watched on that exact date,
			// this film will be placed after that one
			foreach (var film in Enumerable.Reverse(workingMovieArchive))
			{
				var currentMovieDate = new DateOnly(currentMovie.Year.Value, currentMovie.Month.Value, currentMovie.Day.Value);
				var lastFilmDate = new DateOnly(film.Year!.Value, film.Month!.Value, film.Day!.Value);

				if (currentMovieDate >= lastFilmDate)
				{
					currentMovie.Order = film.Order + 1;
					break;
				}
				else
				{
					film.Order += 1;
					continue;
				}
			}

			// Adds buffer Film
			workingMovieArchive.Add(currentMovie);


			currentMovie = new Film();
			isVerified = false;
		}

		public void EraseWorkingArchive()
		{
			workingMovieArchive.Clear();
			safetyCheckMovieArchive.Clear();
		}
	}
}
