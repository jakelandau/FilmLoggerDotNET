using Avalonia.Controls;

namespace FilmLoggerDotNET
{
    public partial class LicensesWindow : Window
    {
        private string iconPath = "../../../Assets/icon.ico";
        public LicensesWindow()
        {
            InitializeComponent();
            Icon = new WindowIcon(iconPath);


        }
    }
}
