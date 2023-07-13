using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FilmLoggerDotNET
{
    public partial class APIKeyWindow : Window
    {
        private string iconPath = "avares://FilmLoggerDotNET/Assets/icon.png";
        public APIKeyWindow()
        {
            InitializeComponent();

            Icon = new WindowIcon(AssetLoader.Open(new System.Uri(iconPath)));

            APIOkButton.Click += OkButtonClick;
        }

        private async void OkButtonClick(object? sender, RoutedEventArgs e)
        {
            // Adds TMDb API Key from text input to dictionary
            Dictionary<string,string> apiKey = new Dictionary<string,string>();
            apiKey.Add("TMDbAPI", TMDbAPIKey.Text);

            // Serializes API Key into secret.json
            string apiKeyJSON = System.Text.Json.JsonSerializer.Serialize(apiKey, new JsonSerializerOptions { WriteIndented = true });
            var secretPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "FilmLoggerDotNET");
            Directory.CreateDirectory(secretPath);
            

            await File.WriteAllTextAsync(Path.Combine(secretPath,"secret.json"), apiKeyJSON);

            // Closes Window
            Close();
        }
    }
}
