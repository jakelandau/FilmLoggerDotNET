using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using JetBrains.Annotations;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;

namespace FilmLogger_DotNET
{
    public partial class MainWindow : Window
    {
        List<Movie> workingMovieArchive;

        // Copy made during list import to check if user has made changes that need to be saved before closing
        List<Movie> safetyCheckMovieArchive; 

        public MainWindow()
        {
            InitializeComponent();

            workingMovieArchive= new List<Movie>();

            Button FilePickerButton = this.FindControl<Button>("FilePicker");
            FilePickerButton.Click += FilePickerButtonClick;

            Button DumpFileButton = this.FindControl<Button>("DumpFile");
            DumpFileButton.Click += DumpFileButtonClick;

        }

        private void FilePickerButtonClick(object? sender, RoutedEventArgs e)
        {
            // Opens FileDialog
            OpenFileDialog FileDialog = new OpenFileDialog();

            // Creates filter to require JSON files
            FileDialogFilter jsonFilter = new FileDialogFilter();
            jsonFilter.Name = "JavaScript Object Notation";
            jsonFilter.Extensions.Add("json");
            FileDialog.Filters.Add(jsonFilter);

            // Try block reads JSON file into working archive
            // If JSON deserializer fails, send to catch block
            try
            {
                //Saves selected file path to string and displays file name
                string filePath = FileDialog.ShowAsync(this).GetAwaiter().GetResult()[0];
                string[] filePathElements = filePath.Split('\\');
                FileName.Text = filePathElements[filePathElements.Length - 1];

                // Deserialized JSON into working Archive
                workingMovieArchive = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(filePath));
                 
                // Copies working archive for dump safety checks
                safetyCheckMovieArchive = new List<Movie>(workingMovieArchive);

                // Updates Film Count Ticker
                UpdateFilmCount();
            }
            catch (NullReferenceException err)
            {  
                // Catches if file dialog is cancelled or closed without a selection
            }
            catch (JsonSerializationException err)
            {
                // TO-DO: Add pop-up notifying that JSON file is invalid
            }
        }

        private void DumpFileButtonClick(object? sender, RoutedEventArgs e)
        {
            // Opens FileDialog with JSON filter to save file
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            FileDialogFilter jsonFilter = new FileDialogFilter();
            jsonFilter.Name = "JavaScript Object Notation";
            jsonFilter.Extensions.Add("json");
            saveFileDialog.Filters.Add(jsonFilter);

            saveFileDialog.ShowAsync(this);
        }

        private void UpdateFilmCount()
        {
            int count = workingMovieArchive.Count;
            ArchiveCount.Text = $"Film Count: {count}";
        }
    }
}
