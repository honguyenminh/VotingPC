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
using VotingPC;

namespace VotingDatabaseMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dialogs dialogs;
        //private static SQLiteAsyncConnection connection;
        private readonly Dictionary<string, Dictionary<string, Candidate>> candidates = new(1);
        private readonly Dictionary<string, Info> sectors = new();
        private string selectedSector, selectedCandidate;

        public MainWindow()
        {
            InitializeComponent();
            dialogs = new(dialogHost);
            SectorList.ItemsSource = sectors.Keys.ToList();

            //NameList.ItemsSource = candidates[0];
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            PaletteHelper paletteHelper = new();
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

        private void AddSectorButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút thêm Sector
        }
        private void AddCandidateButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút thêm ứng cử viên
        }

        private void SectorEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút sửa sector đã chọn
        }
        private void CandidateEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút sửa ứng cử viên đã chọn
        }

        private void SectorRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút xóa Sector
        }
        private void CandidateRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút xóa ứng cử viên
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút xuất file
        }

        private void SectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SectorEditButton.IsEnabled = true;
            var selectedItem = e.AddedItems;
            if (selectedItem.Count == 0)
            {
                selectedSector = null; // TODO: check for this on other use of this var
                NameList.ItemsSource = null;
                SectorEditButton.IsEnabled = false;
                return;
            };
            selectedSector = (string)selectedItem[0];

            NameList.ItemsSource = candidates[selectedSector].Values.ToList();
        }
        private void CandidateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CandidateEditButton.IsEnabled = true;
            var selectedItem = e.AddedItems;
            if (selectedItem.Count == 0)
            {
                selectedCandidate = null;
                CandidateEditButton.IsEnabled = false;
                return;
            };
            selectedCandidate = ((Candidate)selectedItem).Name;
        }

    }

    public class Candidate
    {
        public string Name { get; set; }
        public string Gender { get; set; }
    }
}
