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
        private readonly Dictionary<string, Dictionary<string, Candidate>> candidates = new(1);
        private readonly SectorDictionary sectorDict = new();
        public string SelectedSector { get; set; }
        public string SelectedCandidate { get; set; }

        public ObservableCollection<string> SectorStringList => sectorDict.sectorsIndex;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            dialogs = new(dialogHost);
            SectorList.ItemsSource = sectorDict.sectorsIndex;

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
            _ = sectorDict.Add("Test sector 1", new());
            if (!candidates.ContainsKey("Test sector 1")) candidates.Add("Test sector 1", new());
            AddCandidateButton.IsEnabled = true;
        }
        private async void AddCandidateButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút thêm ứng cử viên
            // TODO: Show add dialog here, somehow :)
            bool result = (bool)await dialogHost.ShowDialog(candidateDialog);
            if (result == false) return;
            Candidate candidate = new()
            {
                Name = candidateDialog.NameInput,
                Gender = candidateDialog.GenderInput
            };
            candidates[SelectedSector].Add(candidate.Name, candidate);
            NameList.ItemsSource = candidates[SelectedSector].Values;
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
            if (SelectedSector == null)
                SectorEditButton.IsEnabled = false;
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
            SelectedCandidate = ((Candidate)selectedItem).Name;
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
