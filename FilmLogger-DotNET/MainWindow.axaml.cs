using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;

using Newtonsoft.Json;
using TMDbLib;

using JetBrains.Annotations;

using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using TMDbLib.Client;
using System.Drawing.Printing;

namespace FilmLogger_DotNET
{
    public partial class MainWindow : Window
    {
        List<Film> workingMovieArchive;
        List<Film> safetyCheckMovieArchive; // Copy made during list import to check if user has made changes that need to be saved before closing
        Dictionary<string, string> APIKeys;
        TMDbClient webClient;
        public MainWindow()
        {
            InitializeComponent();

            workingMovieArchive = new List<Film>();

            Button FilePickerButton = this.FindControl<Button>("FilePicker");
            FilePickerButton.Click += FilePickerButtonClick;

            Button DumpFileButton = this.FindControl<Button>("DumpFile");
            DumpFileButton.Click += DumpFileButtonClick;

            APIKeys = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(@"../../../secret.json"));
            webClient = new(APIKeys["TMDbAPI"]);


        }

        private async void FilePickerButtonClick(object? sender, RoutedEventArgs e)
        {
            // Opens FileDialog
            OpenFileDialog FileDialog = new();

            // Creates filter to require JSON files
            FileDialogFilter jsonFilter = new()
            {
                Name = "JavaScript Object Notation"
            };
            jsonFilter.Extensions.Add("json");
            FileDialog.Filters.Add(jsonFilter);

            // Try block reads JSON file into working archive
            // If JSON deserializer fails, send to catch block
            try
            { 
                // Saves selected file path to string and displays file name
                var filePath = await FileDialog.ShowAsync(this);
                string[] filePathElements = filePath[0].Split('\\');
                FileName.Text = filePathElements[filePathElements.Length - 1];

                string JSONString = await File.ReadAllTextAsync(filePath[0]);

                // Deserialized JSON into working Archive
                workingMovieArchive = JsonConvert.DeserializeObject<List<Film>>(JSONString);
                 
                // Copies working archive for dump safety checks
                safetyCheckMovieArchive = new List<Film>(workingMovieArchive);

                // Updates Film Count Ticker
                UpdateFilmCount();
            }
            catch (NullReferenceException err)
            {  
                // TO-DO: Catches if file dialog is cancelled or closed without a selection
            }
            catch (JsonSerializationException err)
            {
                // TO-DO: Add pop-up notifying that JSON file is invalid
            }
        }

        private async void DumpFileButtonClick(object? sender, RoutedEventArgs e)
        {
            // Opens FileDialog with JSON filter to save file
            SaveFileDialog fileDumpWindow = new();
            FileDialogFilter jsonFilter = new()
            {
                Name = "JavaScript Object Notation"
            };
            jsonFilter.Extensions.Add("json");
            fileDumpWindow.Filters.Add(jsonFilter);

            // Dumps file to selected file path
            string filePath = await fileDumpWindow.ShowAsync(this);
            await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(workingMovieArchive));
        }

        private void UpdateFilmCount()
        {
            int count = workingMovieArchive.Count;
            ArchiveCount.Text = $"Film Count: {count}";
        }
    }
}
