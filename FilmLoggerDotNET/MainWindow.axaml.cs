using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace FilmLoggerDotNET
{
    public partial class MainWindow : Window
    {
        private List<Film> workingMovieArchive = new List<Film>(); // Working archive of all movies user has watched
        private List<Film> safetyCheckMovieArchive = new List<Film>(); // Copy of above made for safety check; if working archive doesn't match safety check, data has not been saved

        private Dictionary<string, string> APIKeys; // Dictionary of API Keys, loaded in and out of secret.json
        private TMDbClient webClient; // Client for accessing TheMovieDB API

        private Film currentMovie = new Film(); // Film object in buffer to add to working archive
        private bool isVerified = false; // Latch to ensure IMDb ID has been verified before attempting to add to archive

        private string iconPath = "avares://FilmLoggerDotNET/Assets/icon.png";
        private string TMDbLogoPath = "avares://FilmLoggerDotNET/Assets/TMDb_logo.png";
        private string blankPosterPath = "avares://FilmLoggerDotNET/Assets/blank_poster.jpg";

        public MainWindow()
        {
            InitializeComponent();
            Icon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)));
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

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
            APIKeyMenu.Click += GetAPIKey;
            LicenseMenu.Click += ShowFilmLoggerLicense;

            // Ensures user has chance to save before closing application
            Closing += MainWindowClosing;
        }


        private async void FilePickerButtonClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);   

            // Try block reads JSON file into working archive
            try
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Open FilmLogger v3 Archive",
                    AllowMultiple = false,
                    FileTypeFilter = new FilePickerFileType[] { new("FilmLogger v3 Archive") { Patterns = new[] { "*.json" }, MimeTypes = new[] { "application/json" } } }
                });

                if (files.Count >= 1)
                {
                    // Grabs JSON string from file on file path and deserializes
                    await using var stream = await files[0].OpenReadAsync();
                    using var streamReader = new StreamReader(stream);
                    var JSONString = await streamReader.ReadToEndAsync();
                    workingMovieArchive = JsonConvert.DeserializeObject<List<Film>>(JSONString);

                    // Grabs file name from file path and displays it
                    FileName.Text = files[0].Name;

                    // Copies working archive for dump safety checks
                    safetyCheckMovieArchive = new List<Film>(workingMovieArchive);

                    // Updates Film Count Ticker
                    UpdateFilmCount();
                }
            }
            // Handles when file has not been selected
            catch (NullReferenceException err)
            {
                // PASS
            }
            // Handles when invalid JSON file is loaded
            catch (JsonSerializationException err)
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                {
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    ContentTitle = "Archive Loading Error",
                    ContentHeader = "Unable to load archive from JSON file",
                    ContentMessage = "Ensure that your JSON file is formatted correctly!",
                    Icon = MsBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
                });
                await errorBox.ShowWindowDialogAsync(this);
            }
        }

        private void DumpFileButtonClick(object? sender, RoutedEventArgs e) => SaveFile();

        private async void VerifyButtonClick(object? sender, RoutedEventArgs e)
        {
            // Checks if API Key is stored on disk, if not calls GetAPIKey and ends function
            if (File.Exists("secret.json"))
            {
                try
                {
                    // Deserializes secret.json to obtain API keys
                    APIKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("secret.json"));

                    // Attempts to instance client using API Key
                    webClient = new TMDbClient(APIKeys["TMDbAPI"]);
                }
                
                catch
                {
                    // Throws up API Key window if stored key doesn't work
                    GetAPIKey(sender,e);
                    return;
                }
            }
            else
            {
                GetAPIKey(sender, e);
                return;
            }

            try
            {

                // Attempts to verify film with TheMovieDB backend,
                // if successful populates initial data into currentMovie buffer
                // and releases verification latch
                Movie webSourcedFilm = await webClient.GetMovieAsync(imdbId: IMDbID.Text);
                currentMovie = new Film
                {
                    imdb_id = webSourcedFilm.ImdbId,
                    title = webSourcedFilm.Title
                };
                isVerified = true;

                // Sets poster image to the TMDb link provided by the API
                PosterImage.Source = $"https://image.tmdb.org/t/p/w500/{webSourcedFilm.PosterPath}";

                // Informs user their film is ready to add to archive
                var successBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                {
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    ContentTitle = "Film Verified!",
                    ContentHeader = "Your film is ready to add to your archive!",
                    ContentMessage = "Make sure the poster on the right matches your film!",
                    Icon = MsBox.Avalonia.Enums.Icon.Success,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
        });
                await successBox.ShowWindowDialogAsync(this);
            }
            catch
            {
                // Clears buffers and re-sets verification latch
                currentMovie = new Film();
                isVerified = false;

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
                await errorBox.ShowWindowDialogAsync(this);
            }
        }

        private async void AddMovieButtonClick(object? sender, RoutedEventArgs e)
        {
            // Ensures film has released verification latch
            if (isVerified)
            {
                if (DateSeen.SelectedDate != null)
                {
                    // Sets parameters of buffer Film
                    currentMovie.theater = (bool)SeenInTheatresBool.IsChecked ? true : false;
                    currentMovie.day = DateSeen.SelectedDate.Value.Day;
                    currentMovie.month = DateSeen.SelectedDate.Value.Month;
                    currentMovie.year = DateSeen.SelectedDate.Value.Year;

                    // Adds buffer Film
                    workingMovieArchive.Add(currentMovie);
                    UpdateFilmCount();

                    // Reset all UI fields and resets buffer and latch
                    PosterImage.Source = blankPosterPath;
                    DateSeen.SelectedDate = null;
                    SeenInTheatresBool.IsChecked = false;
                    IMDbID.Text = "";
                    currentMovie = new Film();
                    isVerified = false;

                    // MessageBox to indicate Film has been added to working archive
                    var successBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                    {
                        ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        ContentTitle = "Film Added to Archive!",
                        ContentHeader = "Your film has been added to the archive!",
                        ContentMessage = "Make sure to save your archive before closing FilmLogger NT 3.51!",
                        Icon = MsBox.Avalonia.Enums.Icon.Success,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
                    });
                    await successBox.ShowWindowDialogAsync(this);
                }
                else
                {
                    // MessageBox to indicate
                    var errorBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                    {
                        ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        ContentTitle = "Date Selection Error",
                        ContentHeader = "You haven't selected a date for when the film was seen!",
                        ContentMessage = "You can't track your films if you don't tell me when you saw it!",
                        Icon = MsBox.Avalonia.Enums.Icon.Error,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
                    });
                    await errorBox.ShowWindowDialogAsync(this);
                }
            }
            // Informs user the verification latch has not been released
            else
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                {
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    ContentTitle = "Film Verification Error",
                    ContentHeader = "You haven't verified your film using an IMDb ID!",
                    ContentMessage = "You need to verify your film before you can log it in your archive!",
                    Icon = MsBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
                });
                await errorBox.ShowWindowDialogAsync(this);
            }
        }

        private async void MainWindowClosing(object sender, CancelEventArgs e)
        {
            // Checks if unsaved film watching data is present
            if (workingMovieArchive.Count > safetyCheckMovieArchive.Count)
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
                var userSelection = await saveBox.ShowWindowDialogAsync(this);

                /* If the user picks yes, saves the file and closes. If the
                 user picks no, sets the working archive equal to the safety
                 archive and closes. Since the counts are equal, the closing
                 process continues without interrupt */
                if (userSelection == "Save archive")
                {
                    SaveFile();
                    Close();
                }
                else
                {
                    workingMovieArchive = safetyCheckMovieArchive;
                    Close();
                }
            }
        }

        private async void SaveFile()
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (workingMovieArchive.Count > safetyCheckMovieArchive.Count)
            {
                // Try block dumps file into selected file path
                try
                {
                    var file = await topLevel.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                    {
                        Title = "Save FilmLogger v3 Archive",
                        FileTypeChoices = new FilePickerFileType[] { new("FilmLogger v3 Archive") { Patterns = new[] { "*.json" }, MimeTypes = new[] { "application/json" } } },
                        ShowOverwritePrompt = true,
                        DefaultExtension = "json"
                    });
                    if (file != null)
                    {
                        await using var stream = await file.OpenWriteAsync();
                        using var streamWriter = new StreamWriter(stream);
                        await streamWriter.WriteLineAsync(JsonConvert.SerializeObject(workingMovieArchive, Formatting.Indented));
                    }

                    // Clears working archive and resets film counter
                    workingMovieArchive = new List<Film>();
                    safetyCheckMovieArchive = new List<Film>();
                    UpdateFilmCount();

                    var successBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                    {
                        ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        ContentTitle = "Archive Saved!",
                        ContentHeader = "Your FilmLogger Archive has been saved!",
                        ContentMessage = "The working archive has now been cleared!",
                        Icon = MsBox.Avalonia.Enums.Icon.Success,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
                    });
                    await successBox.ShowWindowDialogAsync(this);
                }
                // Handles when file has not been selected
                catch (ArgumentNullException err)
                {
                    // PASS
                }
            }
            // Bypasses file dump if no new data has been added
            else if (workingMovieArchive.Count == safetyCheckMovieArchive.Count)
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                {
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    ContentTitle = "Save Error",
                    ContentHeader = "Nothing new added to archive!",
                    ContentMessage = "You haven't added any new films, so there's nothing to save!",
                    Icon = MsBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)))
                });
                await errorBox.ShowWindowDialogAsync(this);
            }
        }

        private void UpdateFilmCount()
        {
            // Updates visual film count indicator on screen
            int count = workingMovieArchive.Count;
            ArchiveCount.Text = $"Film Count: {count}";
        }

        private async void GetAPIKey(object? sender, RoutedEventArgs e)
        {
            // Opens APIKeyWindow to retrieve TheMovieDB API Key from user
            var APIKeyDialog = new APIKeyWindow();
            APIKeyDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            await APIKeyDialog.ShowDialog(this);
        }

        private async void ShowFilmLoggerLicense(object? sender, RoutedEventArgs e)
        {
            // Displays software license for FilmLogger, indicating the program
            // is licensed under the terms of AGPLv3-only.
            var licenseBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                ContentTitle = "FilmLogger License",
                ContentMessage = "Copyright � 2023 Jake Landau\r\n\r\n" +
                "This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License\r\n\r\n as published by the Free Software Foundation, specifically version 3.\r\n\r\n" +
                "This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty\r\n\r\n of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License, version 3 for more details.\r\n\r\n" +
                "A copy of the license is attached in the file `LICENSE.MD`, included in the program folder.",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowIcon = new WindowIcon(AssetLoader.Open(new Uri(iconPath))),
                Markdown = true
            });
            await licenseBox.ShowWindowDialogAsync(this);
        }
    }
}
