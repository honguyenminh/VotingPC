using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SQLite;

namespace VotingDatabaseMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly VotingPC.Dialogs dialogs;
        //private static SQLiteAsyncConnection connection;
        readonly List<List<Candidate>> candidates = new(1);

        public MainWindow()
        {
            InitializeComponent();
            dialogs = new(dialogHost);

            candidates.Add(new());
            candidates[0].Add(new() { Name = "Yoooo test 1" });
            candidates[0].Add(new() { Name = "Yoooo test 2" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            candidates[0].Add(new() { Name = "Yoooo test 3 12831bc0123d1uobid12dbb1ud3o1duoi13d" });
            NameList.ItemsSource = candidates[0];

            List<string> sectors = new();
            sectors.Add("123131231");
            sectors.Add("QH");
            sectors.Add("Tỉnh");
            sectors.Add("Huyện");
            sectors.Add("Xã");
            sectors.Add("Obama");
            sectors.Add("Trần Dần");
            SectorList.ItemsSource = sectors;
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            if (theme.Paper == Theme.Dark.MaterialDesignPaper)
            {
                theme.SetBaseTheme(Theme.Light);
                ThemeIcon.Kind = PackIconKind.WeatherNight;
                ((Button)sender).ToolTip = "Theme màu tối";
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);
                ThemeIcon.Kind = PackIconKind.WeatherSunny;
                ((Button)sender).ToolTip = "Theme màu sáng";
            }

            paletteHelper.SetTheme(theme);
        }
    }

    public class Candidate
    {
        public string Name { get; set; }
        public string Gender { get; set; }
    }
}
