using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace FilmLoggerDotNET
{
    public partial class APIKeyWindow : Window
    {
        private string iconPath = "../../../Assets/icon.ico";
        public APIKeyWindow()
        {
            InitializeComponent();

            Icon = new WindowIcon(iconPath);

            Button okButton = this.FindControl<Button>("APIOkButton");
            okButton.Click += OkButtonClick;
        }

        private async void OkButtonClick(object? sender, RoutedEventArgs e)
        {
            Dictionary<string,string> apiKey = new Dictionary<string,string>();
            apiKey.Add("TMDbAPI", TMDbAPIKey.Text);
            string json = JsonConvert.SerializeObject(apiKey, Formatting.Indented);
            await File.WriteAllTextAsync("secret.json", json);
            Close();
        }
    }
}
