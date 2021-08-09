using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        // This is utterly stupid, DO NOT attempt to do this to anywhere else, I beg you
        // Determine if changes to SectorColor box is made in code or by user
        private static bool _isUserInvoked = true;
        // Code to programmatically change value in SectorColor box. Use this if needed to change it.
        private void ChangeUIColorValue(string newValue)
        {
            _isUserInvoked = false;
            Property.Color = newValue;
            // Clear user invoked flag. This is stupid. But it works.
            // Mission succeeded unsuccessfully.
            _isUserInvoked = true;
        }

        // Public properties for binding to XAML
        public string SelectedSector { get; set; }
        public Candidate SelectedCandidate { get; set; }
        public PropertyBinding Property { get; } = new();
        public ObservableCollection<string> SectorStringList => sectorDict.Keys;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            dialogs = new Dialogs(DialogHost);
            // Disable pasting in SectorMax textbox
            _ = SectorMaxTextBox.CommandBindings.Add(new(ApplicationCommands.Paste, DisablePasting));
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

            bool result = (bool?)await DialogHost.ShowDialog(sectorDialog) ?? false;
            if (result == false) return;

            if (sectorDict.Keys.Contains(sectorDialog.NameInput))
            {
                dialogs.ShowTextDialog("Sector đã tồn tại, vui lòng kiểm tra lại", "OK", customScaleFactor: 1.5);
                return;
            }

            Info info = new()
            {
                Color = "#FFFFFF",
                Sector = sectorDialog.NameInput,
                Title = "",
                Year = ""
            };
            _ = sectorDict.Add(sectorDialog.NameInput, info);
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

            bool result = (bool?)await DialogHost.ShowDialog(candidateDialog) ?? false;
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
            CandidateList.Items.Refresh();
            //CandidateList.ItemsSource = candidates[SelectedSector].Values;
            CandidateList.SelectedItem = candidate;
        }

        private async void SectorEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset dialog
            sectorDialog.Title.Text = "Sửa tên Sector";
            sectorDialog.NameTextBox.Text = SelectedSector;

            bool result = (bool)await DialogHost.ShowDialog(sectorDialog);
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
        private async void CandidateEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset dialog
            candidateDialog.Title.Text = "Sửa ứng cử viên";
            candidateDialog.NameTextBox.Text = SelectedCandidate.Name;
            candidateDialog.GenderBox.Text = SelectedCandidate.Gender;
            bool result = (bool)await DialogHost.ShowDialog(candidateDialog);
            if (result == false) return;

            if (candidates[SelectedSector].Keys.Contains(candidateDialog.NameInput))
            {
                dialogs.ShowTextDialog("Ứng cử viên cùng tên đã tồn tại.", "OK", customScaleFactor: 1.5);
                return;
            }

            // Rename the key in dictionary
            _ = candidates[SelectedSector].Rename(SelectedCandidate.Name, candidateDialog.NameInput);
            // Change values
            SelectedCandidate.Name = candidateDialog.NameInput;
            SelectedCandidate.Gender = candidateDialog.GenderInput;
            CandidateList.Items.Refresh();
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
            // If list is cleared or no item selected
            if (e.AddedItems.Count == 0)
            {
                // Disable edit and remove buttons
                SectorRemoveButton.IsEnabled = false;
                AddCandidateButton.IsEnabled = false;
                SectorEditButton.IsEnabled = false;
                // Clear candidate list
                CandidateList.ItemsSource = null;
                // Disable property panel on the right
                PropertyTitle.IsEnabled = false;
                PropertyPanel.IsEnabled = false;
                // TODO: clear property panel value here, if needed
            }
            else
            {
                // Enable buttons
                AddCandidateButton.IsEnabled = true;
                SectorRemoveButton.IsEnabled = true;
                SectorEditButton.IsEnabled = true;
                // Enable property panel
                PropertyTitle.IsEnabled = true;
                PropertyPanel.IsEnabled = true;
                // Show selected sector's property
                Property.Title = sectorDict[SelectedSector].Title ?? "";
                Property.Max = (sectorDict[SelectedSector].Max ?? 0).ToString();
                Property.Year = sectorDict[SelectedSector].Year ?? "";
                ChangeUIColorValue(sectorDict[SelectedSector].ColorNoHash ?? "");
                // Change candidate list source to selected sector's list
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

        private void SectorTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Property.Title)) return;
            sectorDict[SelectedSector].Title = Property.Title;
        }
        private void SectorYear_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Property.Year)) Property.Year = "";
            sectorDict[SelectedSector].Year = Property.Year;
        }
        // Number only regex
        private static readonly Regex _notNumberOnlyRegex = new("[^0-9]+", RegexOptions.Compiled);
        private void SectorMax_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _notNumberOnlyRegex.IsMatch(e.Text);
        }
        private void DisablePasting(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }
        private void SectorColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            // If no sector selected (which should not trigger this anyway, here just to be extra safe)
            // or changed from code, don't do anything
            if (SelectedSector is null || !_isUserInvoked) return;

            // If validation failed aka invalid color
           if (Validation.GetHasError((TextBox)sender))
            {
                sectorDict[SelectedSector].Color = null;
                return;
            }
            sectorDict[SelectedSector].ColorNoHash = Property.Color;
        }
    }

    public class Candidate
    {
        public string Name { get; set; }
        public string Gender { get; set; }
    }
}
