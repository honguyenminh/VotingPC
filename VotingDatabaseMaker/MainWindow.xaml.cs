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
using System.Collections.ObjectModel;

namespace VotingDatabaseMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dialogs dialogs;
        private readonly CandidateDialog candidateDialog = new();
        //private static SQLiteAsyncConnection connection;
        private readonly ObservableDictionary<string, Dictionary<string, Candidate>> candidates = new();
        private readonly ObservableDictionary<string, Info> sectorDict = new();
        public string SelectedSector { get; set; }
        public Candidate SelectedCandidate { get; set; }

        // Public properties for binding to XAML
        public string SectorTitle { get; set; }
        public string SectorYear { get; set; }
        public string SectorMax { get; set; }
        public Color SectorColor { get; set; }
        public ObservableCollection<string> SectorStringList => sectorDict.Keys;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            dialogs = new(dialogHost);
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
            _ = sectorDict.Add("Test sector 1", new());
            if (!candidates.Add("Test sector 1", new()))
            {
                dialogs.ShowTextDialog("Sector đã tồn tại, vui lòng kiểm tra lại", "OK");
                return;
            }
            SectorList.SelectedItem = "Test sector 1";
            AddCandidateButton.IsEnabled = true;
        }
        private async void AddCandidateButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút thêm ứng cử viên
            bool result = (bool)await dialogHost.ShowDialog(candidateDialog);
            if (result == false) return;
            if (candidates.GetValue(SelectedSector).ContainsKey(candidateDialog.NameInput))
            {
                dialogs.ShowTextDialog("Ứng cử viên đã tồn tại", "OK");
                return;
            }
            Candidate candidate = new()
            {
                Name = candidateDialog.NameInput,
                Gender = candidateDialog.GenderInput
            };
            candidates.GetValue(SelectedSector).Add(candidate.Name, candidate);
            NameList.ItemsSource = candidates.GetValue(SelectedSector).Values.ToList();
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
            // TODO: add enable and disable sector property here
            NameList.ItemsSource = candidates.GetValue(SelectedSector).Values.ToList();
            //if (SelectedSector == null)
        }
        private void CandidateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CandidateEditButton.IsEnabled = true;
            var selectedItem = e.AddedItems;
            if (selectedItem.Count == 0)
            {
                SelectedCandidate = null;
                CandidateEditButton.IsEnabled = false;
                return;
            };
        }
        private void SectorList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SectorList.Items.Refresh();
        }
    }

    public class Candidate
    {
        public string Name { get; set; }
        public string Gender { get; set; }
    }
}
