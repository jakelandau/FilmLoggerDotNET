using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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

            // Fallback due to lack of Mica brush on Windows 10, Linux and Mac
            if (!System.OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
            {
                TransparencyLevelHint = new List<WindowTransparencyLevel>() { WindowTransparencyLevel.None };
                var BackgroundGradient = new LinearGradientBrush()
                {
                    GradientStops = {
                        new GradientStop(offset: 0.0, color: Color.FromRgb(55, 6, 23)),
                        new GradientStop(offset: 0.25, color: Color.FromRgb(57, 6, 22)),
                        new GradientStop(offset: 0.50, color: Color.FromRgb(60, 6, 20)),
                        new GradientStop(offset: 0.75, color: Color.FromRgb(62, 5, 19)),
                        new GradientStop(offset: 1.00, color: Color.FromRgb(64, 5, 17))
                    },
                    StartPoint = Avalonia.RelativePoint.TopLeft,
                    EndPoint = Avalonia.RelativePoint.BottomRight
                };
                Background = BackgroundGradient;
            }

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
