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
        private readonly SectorDialog sectorDialog = new();
        private readonly ObservableDictionary<string, ObservableDictionary<string, Candidate>> candidates = new();
        private readonly ObservableDictionary<string, Info> sectorDict = new();

        // Public properties for binding to XAML
        public string SelectedSector { get; set; }
        public Candidate SelectedCandidate { get; set; }
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

        private async void AddSectorButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset dialog
            sectorDialog.Title.Text = "Thêm Sector";
            sectorDialog.NameTextBox.Text = "";

            bool result = (bool)await dialogHost.ShowDialog(sectorDialog);
            if (result == false) return;

            if (!sectorDict.Add(sectorDialog.NameInput, new()))
            {
                dialogs.ShowTextDialog("Sector đã tồn tại, vui lòng kiểm tra lại", "OK", customScaleFactor: 1.5);
                return;
            }

            _ = candidates.Add(sectorDialog.NameInput, new());
            SectorList.SelectedItem = sectorDialog.NameInput;
            AddCandidateButton.IsEnabled = true;
        }
        private async void AddCandidateButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset dialog
            candidateDialog.Title.Text = "Thêm ứng cử viên";
            candidateDialog.NameTextBox.Text = "";
            candidateDialog.GenderBox.Text = "";

            bool result = (bool)await dialogHost.ShowDialog(candidateDialog);
            if (result == false) return;

            if (candidates[SelectedSector].Keys.Contains(candidateDialog.NameInput))
            {
                dialogs.ShowTextDialog("Ứng cử viên đã tồn tại", "OK", customScaleFactor: 1.5);
                return;
            }

            Candidate candidate = new()
            {
                Name = candidateDialog.NameInput,
                Gender = candidateDialog.GenderInput
            };
            _ = candidates[SelectedSector].Add(candidate.Name, candidate);
            CandidateList.ItemsSource = candidates[SelectedSector].Values;
        }

        private async void SectorEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset dialog
            sectorDialog.Title.Text = "Sửa tên Sector";
            sectorDialog.NameTextBox.Text = SelectedSector;

            bool result = (bool)await dialogHost.ShowDialog(sectorDialog);
            if (result == false) return;

            if (sectorDict.Keys.Contains(sectorDialog.NameInput))
            {
                dialogs.ShowTextDialog("Sector cùng tên đã tồn tại.", "OK", customScaleFactor: 1.5);
                return;
            }

            _ = candidates.Rename(SelectedSector, sectorDialog.NameInput);
            _ = sectorDict.Rename(SelectedSector, sectorDialog.NameInput);
            // Change selected sector to reflect renamed item
            SectorList.SelectedItem = sectorDialog.NameInput;
            // Change candidate list item source to renamed item
            CandidateList.ItemsSource = candidates[SelectedSector].Values;
        }
        private void CandidateEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset dialog
            candidateDialog.Title.Text = "Sửa ứng cử viên";
            candidateDialog.NameTextBox.Text = SelectedCandidate.Name;
            candidateDialog.GenderBox.Text = SelectedCandidate.Gender;
        }

        private void SectorRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút xóa Sector
            _ = candidates.RemoveKey(SelectedSector);
            _ = sectorDict.RemoveKey(SelectedSector);
        }
        private void CandidateRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút xóa ứng cử viên
            _ = candidates[SelectedSector].RemoveKey(SelectedCandidate.Name);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút xuất file
        }

        private void SectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: add enable and disable sector property here
            if (e.AddedItems.Count == 0)
            {
                SectorRemoveButton.IsEnabled = false;
                AddCandidateButton.IsEnabled = false;
                SectorEditButton.IsEnabled = false;
                CandidateList.ItemsSource = null;
            }
            else
            {
                AddCandidateButton.IsEnabled = true;
                SectorRemoveButton.IsEnabled = true;
                SectorEditButton.IsEnabled = true;
                CandidateList.ItemsSource = candidates[SelectedSector].Values;
            }
        }
        private void CandidateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                SelectedCandidate = null;
                CandidateEditButton.IsEnabled = false;
                CandidateRemoveButton.IsEnabled = false;
                return;
            }
            CandidateEditButton.IsEnabled = true;
            CandidateRemoveButton.IsEnabled = true;
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
