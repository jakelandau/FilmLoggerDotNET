using AsyncImageLoader.Loaders;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FilmLoggerDotNET
{
	public partial class MainWindow : Window
	{
		// Dictionary of API Keys, loaded in and out of secret.json
		private Dictionary<string, string> APIKeys = new Dictionary<string, string>();

		// Calls BusinessLogic.cs as logic processor
		private BusinessLogic logicProcessor = new BusinessLogic();

		private readonly string iconPath = "avares://FilmLoggerDotNET/Assets/icon.png";
		private readonly string TMDbLogoPath = "avares://FilmLoggerDotNET/Assets/TMDb_logo.png";
		private readonly string blankPosterPath = "avares://FilmLoggerDotNET/Assets/blank_poster.png";

		public MainWindow()
		{
			InitializeComponent();
			Icon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)));
			WindowStartupLocation = WindowStartupLocation.CenterScreen;

			// Fallback due to lack of Mica brush on Windows 10, Linux and Mac
			if (!System.OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
			{
				Background = new SolidColorBrush(color: Color.FromRgb(30, 30, 35));
			}

			// Sets the time dial to ensure that the date the film is seen
			// is set between the Unix Epoch and the current year
			DateSeen.MaxYear = DateTimeOffset.Now;
			DateSeen.MinYear = DateTimeOffset.UnixEpoch;

			// Associates Buttons with event handlers
			FilePicker.Click += FilePickerButtonClick;
			DumpFile.Click += DumpFileButtonClick;
			VerifyButton.Click += VerifyButtonClick;
			AddMovie.Click += AddMovieButtonClick;

			// Associates Menu items with event handlers
			DumpFileMenu.Click += DumpFileButtonClick;
			OpenFileMenu.Click += FilePickerButtonClick;
			LicenseMenu.Click += ShowFilmLoggerLicense;

			// Ensures user has chance to save before closing application
			Closing += MainWindowClosing;

			// Sets Poster Image to cache posters to disk in AppData/$HOME
			PosterImage.Loader = new DiskCachedWebImageLoader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "FilmLoggerDotNET", "Cache"));
		}


		private async void FilePickerButtonClick(object? sender, RoutedEventArgs e)
		{
			var topLevel = TopLevel.GetTopLevel(this);

			// Try block reads JSON file into working archive
			try
			{
				var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
				{
					Title = "Open FilmLogger v3 Archive",
					AllowMultiple = false,
					FileTypeFilter = new FilePickerFileType[] { new("FilmLogger v3 Archive") { Patterns = new[] { "*.json" }, MimeTypes = new[] { "application/json" } } }
				});

				if (files.Count >= 1)
				{
					// Passes bytestream to business logic for deserialization
					await using var stream = await files[0].OpenReadAsync();
					await logicProcessor.ReadArchiveFileAsync(stream);

					// Grabs file name from file path and displays it
					FileName.Text = files[0].Name;

					// Updates Film Count Ticker
					UpdateFilmCount();
				}
			}
			// Handles when file has not been selected
			catch (NullReferenceException)
			{
				// PASS
			}
			// Handles when invalid JSON file is loaded
			catch (JsonException)
			{
				await ShowMessageBoxAsync(isCustom: false,
					contentTitle: "Archive Loading Error",
					contentHeader: "Unable to load archive from JSON file",
					contentMessage: "Ensure that your JSON file is formatted correctly!",
					boxIcon: MsBox.Avalonia.Enums.Icon.Error);
			}
		}

		private async void DumpFileButtonClick(object? sender, RoutedEventArgs e) => await SaveFileAsync();

		private async void VerifyButtonClick(object? sender, RoutedEventArgs e)
		{
			try
			{
				// Sets poster image to the TMDb link provided by the API.
				// Null possibilities for the text field are ignored as the
				// VerifyFilmAsync function throws an exception that is caught
				PosterImage.Source = await logicProcessor.VerifyFilmAsync(IMDbSearchKey: IMDbID.Text!);

				await ShowMessageBoxAsync(isCustom: false,
					contentTitle: "Film Verified!",
					contentHeader: "Your film is ready to add to your archive!",
					contentMessage: "Make sure the poster on the right matches your film!",
					boxIcon: MsBox.Avalonia.Enums.Icon.Success
					);
			}
			catch
			{
				// Sets poster back to blank
				PosterImage.Source = blankPosterPath;

				// Informs user of failure and provides troubleshooting steps
				var errorBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
				{
					ButtonDefinitions = new[]
					{
						new ButtonDefinition {Name = "Ok",IsDefault=true}
					},
					ContentTitle = "Film Verification Error",
					ContentHeader = "Unable to verify with TMDb that film exists!",
					ContentMessage = "Check that: \r\n\r\n" + "-Your IMDb ID is valid!\r\n\r\n" + "-Both you and themoviedb.org are online!\r\n\r\n",
					ImageIcon = new Bitmap(AssetLoader.Open(new Uri(TMDbLogoPath))),
					WindowStartupLocation = WindowStartupLocation.CenterScreen,
					SizeToContent = SizeToContent.WidthAndHeight,
					WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
				});
				await errorBox.ShowAsPopupAsync(this);
			}
		}

		private async void AddMovieButtonClick(object? sender, RoutedEventArgs e)
		{
			// Ensures film has released verification latch
			if (logicProcessor.IsVerified())
			{
				if (DateSeen.SelectedDate != null)
				{
					// Adds film to working archive, since checkboxes are nullable
					// we use the null-coalescing operator to assign false if null
					logicProcessor.AddFilmToArchive(inTheater: SeenInTheatresBool.IsChecked ?? false,
						Day: DateSeen.SelectedDate.Value.Day,
						Month: DateSeen.SelectedDate.Value.Month,
						Year: DateSeen.SelectedDate.Value.Year);

					// Reset all UI fields and resets buffer and latch
					UpdateFilmCount();
					PosterImage.Source = blankPosterPath;
					DateSeen.SelectedDate = null;
					SeenInTheatresBool.IsChecked = false;
					IMDbID.Text = "";

					// MessageBox to indicate Film has been added to working archive
					await ShowMessageBoxAsync(isCustom: false,
						contentTitle: "Film Added to Archive!",
						contentHeader: "Your film has been added to the archive!",
						contentMessage: "Make sure to save your archive before closing FilmLogger NT 3.51!",
						boxIcon: MsBox.Avalonia.Enums.Icon.Success);
				}
				else
				{
					// Informs user they have not selected a date for the Film
					await ShowMessageBoxAsync(isCustom: false,
						contentTitle: "Date Selection Error",
						contentHeader: "You haven't selected a date for when the film was seen!",
						contentMessage: "You can't track your films if you don't tell me when you saw it!",
						boxIcon: MsBox.Avalonia.Enums.Icon.Error);
				}
			}
			// Informs user the verification latch has not been released
			else
			{
				await ShowMessageBoxAsync(isCustom: false,
					contentTitle: "Film Verification Error",
					contentHeader: "You haven't verified your film using an IMDb ID!",
					contentMessage: "You need to verify your film before you can log it in your archive!",
					boxIcon: MsBox.Avalonia.Enums.Icon.Error);
			}
		}

		private async void MainWindowClosing(object? sender, CancelEventArgs e)
		{
			// Checks if unsaved film watching data is present
			if (logicProcessor.WorkingMovieArchive().Count > logicProcessor.SafetyCheckMovieArchive().Count)
			{
				// Cancels window close event
				e.Cancel = true;

				// Creates dialog box asking if the user wants to save changes
				var saveBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
				{
					ButtonDefinitions = new[] {
						new ButtonDefinition {Name = "Save archive", IsDefault = true},
						new ButtonDefinition {Name = "Close without saving", IsCancel = true}
					},
					ContentTitle = "Archive Not Saved!",
					ContentHeader = "You haven't saved your changes yet!",
					ContentMessage = "Do you want to save your archive?",
					Icon = MsBox.Avalonia.Enums.Icon.Question,
					WindowStartupLocation = WindowStartupLocation.CenterScreen,
					WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
				});
				var userSelection = await saveBox.ShowAsPopupAsync(this);

				/* If the user picks yes, saves the file and closes. If the
                 user picks no, sets the working archive equal to the safety
                 archive and closes. Since the counts are equal, the closing
                 process continues without interrupt */
				if (userSelection == "Save archive")
				{
					await SaveFileAsync();
					Close();
				}
				else
				{
					logicProcessor.EraseWorkingArchive();
					UpdateFilmCount();
					Close();
				}
			}
		}

		private async Task SaveFileAsync()
		{
			var topLevel = TopLevel.GetTopLevel(this);

			if (logicProcessor.WorkingMovieArchive().Count > logicProcessor.SafetyCheckMovieArchive().Count)
			{
				// Try block dumps file into selected file path
				try
				{
					// File Selector for choosing save path
					var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
					{
						Title = "Save FilmLogger v3 Archive",
						FileTypeChoices = new FilePickerFileType[] { new("FilmLogger v3 Archive") { Patterns = new[] { "*.json" }, MimeTypes = new[] { "application/json" } } },
						ShowOverwritePrompt = true,
						DefaultExtension = "json"
					});

					if (file != null)
					{
						await logicProcessor.SaveArchiveFileAsync(file);

						// Resets film counter
						UpdateFilmCount();

						// Notifies user of success in saving archive
						await ShowMessageBoxAsync(isCustom: false,
							contentTitle: "Archive Saved!",
							contentHeader: "Your FilmLogger Archive has been saved!",
							contentMessage: "The working archive has now been cleared!",
							boxIcon: MsBox.Avalonia.Enums.Icon.Success);
					}
					// Handles when file dialog has been cancelled without saving
					else
					{
						// PASS
					}
				}
				// Handles when file has not been selected
				catch (ArgumentNullException)
				{
					// PASS
				}
			}
			// Bypasses file dump if no new data has been added
			else if (logicProcessor.WorkingMovieArchive().Count == logicProcessor.SafetyCheckMovieArchive().Count)
			{
				await ShowMessageBoxAsync(isCustom: false,
					contentTitle: "Save Error",
					contentHeader: "Nothing new added to archive!",
					contentMessage: "You haven't added any new films, so there's nothing to save!",
					boxIcon: MsBox.Avalonia.Enums.Icon.Error
					);
			}
		}

		private void UpdateFilmCount()
		{
			// Updates visual film count indicator on screen
			int count = logicProcessor.WorkingMovieArchive().Count;
			ArchiveCount.Text = $"Film Count: {count}";
		}

		private async void ShowFilmLoggerLicense(object? sender, RoutedEventArgs e)
		{
			// Displays software license for FilmLogger, indicating the program
			// is licensed under the terms of AGPLv3
			await ShowMessageBoxAsync(isCustom: false,
				contentTitle: "FilmLogger License",
				contentHeader: "",
				contentMessage: "Copyright 2023 Jake Landau\r\n\r\n" +
				"This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License\r\n\r\n as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.\r\n\r\n" +
				"This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty\r\n\r\n of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License, version 3 for more details.\r\n\r\n" +
				"A copy of the license is attached in the file `LICENSE.MD`, included in the program folder.",
				boxIcon: MsBox.Avalonia.Enums.Icon.Info);
		}

		private async Task ShowMessageBoxAsync(bool isCustom, string contentTitle, string contentHeader, string contentMessage, MsBox.Avalonia.Enums.Icon boxIcon)
		{
			if (isCustom)
			{
				throw new NotImplementedException();
			}
			else
			{
				var msgBoxParams = new MessageBoxStandardParams
				{
					WindowStartupLocation = WindowStartupLocation.CenterScreen,
					WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath))),
					ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.Ok,
					EnterDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Ok,
					Icon = boxIcon,
					ContentTitle = contentTitle,
					ContentHeader = contentHeader,
					ContentMessage = contentMessage,
					Markdown = true
				};
				var msgBox = MessageBoxManager.GetMessageBoxStandard(msgBoxParams);
				await msgBox.ShowAsPopupAsync(this);
			}

		}
	}
}
