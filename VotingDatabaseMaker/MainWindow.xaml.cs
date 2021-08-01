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
        private readonly ObservableDictionary<string, ObservableDictionary<string, Candidate>> candidates = new();
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

        private async void AddSectorButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút thêm Sector
            StackPanel stackPanel = new() { Margin = new(16) };
            TextBlock title = new()
            {
                Text = "Nhập tên sector (cấp bầu cử, chức vụ,...)",
                FontSize = 20
            };
            TextBox nameBox = new()
            {
                Margin = new(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            StackPanel buttonStack = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Button cancelButton = new()
            {
                Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"],
                IsCancel = true,
                Margin = new(0, 8, 8, 0),
                Content = "Hủy"
            };
            cancelButton.Click += (sender, e) => dialogs.CloseDialog();
            Button submitButton = new()
            {
                Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"],
                IsDefault = true,
                Margin = new(0, 8, 8, 0),
                Content = "OK"
            };
            submitButton.Click += (sender, e) => dialogs.CloseDialog();
            _ = buttonStack.Children.Add(cancelButton);
            _ = buttonStack.Children.Add(submitButton);
            _ = stackPanel.Children.Add(title);
            _ = stackPanel.Children.Add(nameBox);
            _ = stackPanel.Children.Add(buttonStack);
            _ = await dialogHost.ShowDialog(stackPanel);

            if (!sectorDict.Add(nameBox.Text, new()))
            {
                dialogs.ShowTextDialog("Sector đã tồn tại, vui lòng kiểm tra lại", "OK", customScaleFactor: 1.5);
                return;
            }
            candidates.Add(nameBox.Text, new());
            SectorList.SelectedItem = nameBox.Text;
            AddCandidateButton.IsEnabled = true;
        }
        private async void AddCandidateButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút thêm ứng cử viên
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
            NameList.ItemsSource = candidates[SelectedSector].Values;
        }

        private void SectorEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút sửa cấp đã chọn
            // SelectedSector is sectoredit
            StackPanel stackPanel = new()
            {
                Margin = new(16),
                Width = 220,
            };
            TextBlock title = new()
            {
                Text = "Sửa cấp",
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            TextBox nameBox = new()
            {
                Margin = new(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            nameBox.Text = nameBox.Text.Insert(0, SelectedSector);
            StackPanel buttonStack = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Button cancelButton = new()
            {
                Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"],
                IsCancel = true,
                Margin = new(0, 8, 8, 0),
                Content = "Hủy"
            };
            cancelButton.Click += (sender, e) => dialogs.CloseDialog();
            Button submitButton = new()
            {
                Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"],
                IsDefault = true,
                Margin = new(0, 8, 8, 0),
                Content = "OK"
            };
            submitButton.Click += (sender, e) =>
            {
                dialogs.CloseDialog();
                if (sectorDict.Keys.Contains(nameBox.Text))
                {
                    dialogs.ShowTextDialog("Sector cùng tên đã tồn tại.", "OK", customScaleFactor: 1.5);
                    return;
                }
                _ = candidates.Rename(SelectedSector, nameBox.Text);
                _ = sectorDict.Rename(SelectedSector, nameBox.Text);
                SelectedSector = nameBox.Text;
                NameList.ItemsSource = candidates[SelectedSector].Values;
            };
            _ = buttonStack.Children.Add(cancelButton);
            _ = buttonStack.Children.Add(submitButton);
            _ = stackPanel.Children.Add(title);
            _ = stackPanel.Children.Add(nameBox);
            _ = stackPanel.Children.Add(buttonStack);
            _ = dialogHost.ShowDialog(stackPanel);
        }
        private void CandidateEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Nút sửa ứng cử viên đã chọn
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
                NameList.ItemsSource = null;
            }
            else
            {
                AddCandidateButton.IsEnabled = true;
                SectorRemoveButton.IsEnabled = true;
                SectorEditButton.IsEnabled = true;
                NameList.ItemsSource = candidates[SelectedSector].Values;
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
