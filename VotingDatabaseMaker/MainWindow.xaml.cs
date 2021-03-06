using MaterialDesignThemes.Wpf;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SQLite;
using VotingPC.Domain;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using AsyncDialog;
using VotingPC.Domain.Extensions;

namespace VotingDatabaseMaker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly AsyncDialogManager _dialogs;
    private readonly CandidateDialog _candidateDialog = new();
    private readonly SectorDialog _sectorDialog = new();
    private readonly PasswordDialog _passwordDialog = new();
    private readonly ObservableDictionary<string, ObservableDictionary<string, Candidate>> _candidates = new();
    private readonly ObservableDictionary<string, Sector> _sectorDict = new();

    // This is utterly stupid, DO NOT attempt to do this to anywhere else, I beg you
    // Determine if changes to SectorColor box is made in code or by user
    private static bool s_isUserInvoked = true;
    // Code to programmatically change value in SectorColor box. Use this if needed to change it.
    private void ChangeUiColorValue(string newValue)
    {
        s_isUserInvoked = false;
        Property.Color = newValue;
        // Clear user invoked flag. This is stupid. But it works.
        // Mission succeeded unsuccessfully.
        s_isUserInvoked = true;
    }

    // Public properties for binding to XAML
    public string SelectedSector { get; set; }
    public Candidate SelectedCandidate { get; set; }
    public PropertyBinding Property { get; } = new();
    public ObservableCollection<string> SectorStringList => _sectorDict.Keys;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _dialogs = new AsyncDialogManager(dialogHost);
        // Disable pasting in SectorMax textbox
        _ = sectorMaxTextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste,
            (_, e) => e.Handled = true));
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();
        if (theme.Paper == Theme.Dark.MaterialDesignPaper)
        {
            theme.SetBaseTheme(Theme.Light);
            themeIcon.Kind = PackIconKind.WeatherNight;
            ((Button)sender).ToolTip = "Theme màu tối";
        }
        else
        {
            theme.SetBaseTheme(Theme.Dark);
            themeIcon.Kind = PackIconKind.WeatherSunny;
            ((Button)sender).ToolTip = "Theme màu sáng";
        }

        paletteHelper.SetTheme(theme);
    }

    private async void AddSectorButton_Click(object sender, RoutedEventArgs e)
    {
        // Reset dialog
        _sectorDialog.title.Text = "Thêm Sector";
        _sectorDialog.nameTextBox.Text = "";

        bool result = (bool?)await dialogHost.ShowDialog(_sectorDialog) ?? false;
        if (result == false) return;

        if (_sectorDict.Keys.Contains(_sectorDialog.NameInput))
        {
            await _dialogs.ShowTextDialog("Sector đã tồn tại, vui lòng kiểm tra lại");
            return;
        }

        if (_sectorDialog.NameInput.StartsWith("sqlite_"))
        {
            await _dialogs.ShowTextDialog("Tên sector không hợp lệ, chứa từ khóa cấm 'sqlite_'");
            return;
        }

        Sector info = new()
        {
            Color = "#FFFFFF",
            Name = _sectorDialog.NameInput,
            Title = "",
            Subtitle = "",
            Max = 0
        };
        _ = _sectorDict.Add(_sectorDialog.NameInput, info);
        _ = _candidates.Add(_sectorDialog.NameInput, new ObservableDictionary<string, Candidate>());
        sectorList.SelectedItem = _sectorDialog.NameInput;
        addCandidateButton.IsEnabled = true;
    }
    private async void AddCandidateButton_Click(object sender, RoutedEventArgs e)
    {
        // Reset dialog
        _candidateDialog.title.Text = "Thêm ứng cử viên";
        _candidateDialog.nameTextBox.Text = "";
        _candidateDialog.genderBox.Text = "";

        bool result = (bool?)await dialogHost.ShowDialog(_candidateDialog) ?? false;
        if (result == false) return;

        if (_candidates[SelectedSector].Keys.Contains(_candidateDialog.NameInput))
        {
            await _dialogs.ShowTextDialog("Ứng cử viên đã tồn tại");
            return;
        }

        Candidate candidate = new()
        {
            Name = _candidateDialog.NameInput,
            Gender = _candidateDialog.GenderInput
        };
        _ = _candidates[SelectedSector].Add(candidate.Name, candidate);
        candidateList.Items.Refresh();
        candidateList.SelectedItem = candidate;
    }

    private async void SectorEditButton_Click(object sender, RoutedEventArgs e)
    {
        // Reset dialog
        _sectorDialog.title.Text = "Sửa tên Sector";
        _sectorDialog.nameTextBox.Text = SelectedSector;

        bool result = (bool)(await dialogHost.ShowDialog(_sectorDialog))!;
        if (result == false) return;

        if (_sectorDict.Keys.Contains(_sectorDialog.NameInput))
        {
            await _dialogs.ShowTextDialog("Sector cùng tên đã tồn tại.");
            return;
        }

        if (_sectorDialog.NameInput.StartsWith("sqlite_"))
        {
            await _dialogs.ShowTextDialog("Tên sector không hợp lệ, chứa từ khóa cấm 'sqlite_'");
            return;
        }

        _ = _candidates.Rename(SelectedSector, _sectorDialog.NameInput);
        _ = _sectorDict.Rename(SelectedSector, _sectorDialog.NameInput);
        _sectorDict[_sectorDialog.NameInput].Name = _sectorDialog.NameInput;
        // Change selected sector to reflect renamed item
        sectorList.SelectedItem = _sectorDialog.NameInput;
        // Change candidate list item source to renamed item
        candidateList.ItemsSource = _candidates[SelectedSector].Values;
    }
    private async void CandidateEditButton_Click(object sender, RoutedEventArgs e)
    {
        // Reset dialog
        _candidateDialog.title.Text = "Sửa ứng cử viên";
        _candidateDialog.nameTextBox.Text = SelectedCandidate.Name;
        _candidateDialog.genderBox.Text = SelectedCandidate.Gender;
        bool result = (bool)(await dialogHost.ShowDialog(_candidateDialog))!;
        if (result == false) return;

        // Rename the key in dictionary
        _ = _candidates[SelectedSector].Rename(SelectedCandidate.Name, _candidateDialog.NameInput);
        // Change values
        SelectedCandidate.Name = _candidateDialog.NameInput;
        SelectedCandidate.Gender = _candidateDialog.GenderInput;
        candidateList.Items.Refresh();
    }

    private void RemoveSectorButton_Click(object sender, RoutedEventArgs e)
    {
        _ = _candidates.RemoveKey(SelectedSector);
        _ = _sectorDict.RemoveKey(SelectedSector);
    }
    private void RemoveCandidateButton_Click(object sender, RoutedEventArgs e)
    {
        _ = _candidates[SelectedSector].RemoveKey(SelectedCandidate.Name);
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        // Show loading dialog
        _dialogs.ShowLoadingDialog("Kiểm tra thông tin đã nhập");
        // Validate current inputted values
        List<string> invalidSectors = new();
        foreach (Sector info in _sectorDict.Values)
        {
            if (!info.IsValid)
            {
                invalidSectors.Add(info.Name);
            }
        }

        if (invalidSectors.Count != 0)
        {
            string errorMessage = string.Join(", ", invalidSectors);
            await _dialogs.CloseDialog();
            await _dialogs.ShowTextDialog(errorMessage, "Thiếu thông tin trong các sector");
            return;
        }

        // Save data to files
        await _dialogs.CloseDialog();
        await _dialogs.ShowTextDialog("Chọn nơi lưu file cơ sở dữ liệu");
        // Ask where to save file
        SaveFileDialog saveFileDialog = new()
        {
            Title = "Chọn nơi lưu file",
            Filter = "Database file|*.db",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        };
        if (saveFileDialog.ShowDialog() == false) return;

        // TODO: swap for new password dialog from asyncdialog
        // Ask password
        bool result = (bool)(await dialogHost.ShowDialog(_passwordDialog))!;
        if (!result)
        {
            // Reset the password box, you don't want old password to be there
            _passwordDialog.passwordTextBox.Password = "";
            return;
        }

        _dialogs.ShowLoadingDialog("Lưu dữ liệu đã nhập vào tệp");

        string path = saveFileDialog.FileName;

        // TODO: Check if we have write priviledge
        // Delete file if already exists
        try { if (File.Exists(path)) File.Delete(path); }
        // In case file is write-locked
        catch
        {
            await _dialogs.ShowTextDialog("File cơ sở dữ liệu đã tồn tại và đang được\n" +
                                   "sử dụng bởi ứng dụng khác, không thế ghi đè.\n" +
                                   "Đóng ứng dụng khác có thể đang sử dụng file và thử lại.");
            return;
        }

        // Create new SQLite connection, with given password and file path
        SQLiteConnectionString option = new(saveFileDialog.FileName, 
            true, _passwordDialog.passwordTextBox.Password);
        SQLiteAsyncConnection connection = new(option);

        // Create Info table then add all info row to table
        _ = await connection.CreateTableAsync<Sector>();
        _ = await connection.InsertAllAsync(_sectorDict.Values);

        // Create Sector tables then add candidates for each sectors
        foreach (string sector in _candidates.Keys)
        {
            await connection.CreateCandidateTableAsync(sector);
            if (_candidates[sector].Values.Count > 0)
                await connection.InsertAllCandidateAsync(sector, _candidates[sector].Values);
        }

        await connection.CloseAsync();
        await _dialogs.CloseDialog();
        await _dialogs.ShowTextDialog("Hoàn tất!");
    }

    private void SectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // If list is cleared or no item selected
        if (e.AddedItems.Count == 0)
        {
            // Disable edit and remove buttons
            sectorRemoveButton.IsEnabled = false;
            addCandidateButton.IsEnabled = false;
            sectorEditButton.IsEnabled = false;
            // Clear candidate list
            candidateList.ItemsSource = null;
            // Disable property panel on the right
            propertyTitle.IsEnabled = false;
            propertyPanel.IsEnabled = false;
            // TODO: clear property panel value here, if needed
        }
        else
        {
            // Enable buttons
            addCandidateButton.IsEnabled = true;
            sectorRemoveButton.IsEnabled = true;
            sectorEditButton.IsEnabled = true;
            // Enable property panel
            propertyTitle.IsEnabled = true;
            propertyPanel.IsEnabled = true;
            // Show selected sector's property
            Property.Title = _sectorDict[SelectedSector].Title ?? "";
            Property.Max = (_sectorDict[SelectedSector].Max ?? 0).ToString();
            Property.Subtitle = _sectorDict[SelectedSector].Subtitle ?? "";
            ChangeUiColorValue(_sectorDict[SelectedSector].ColorNoHash ?? "");
            // Change candidate list source to selected sector's list
            candidateList.ItemsSource = _candidates[SelectedSector].Values;
        }
    }
    private void CandidateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            SelectedCandidate = null;
            candidateEditButton.IsEnabled = false;
            candidateRemoveButton.IsEnabled = false;
            return;
        }
        candidateEditButton.IsEnabled = true;
        candidateRemoveButton.IsEnabled = true;
    }

    private void SectorTitle_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Property.Title)) return;
        _sectorDict[SelectedSector].Title = Property.Title;
    }
    private void SectorSubtitle_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(Property.Subtitle)) Property.Subtitle = "";
        _sectorDict[SelectedSector].Subtitle = Property.Subtitle;
    }
    // Number only regex
    private static readonly Regex s_notNumberOnlyRegex = new("[^0-9]+", RegexOptions.Compiled);
    private void SectorMax_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        bool invalid = s_notNumberOnlyRegex.IsMatch(e.Text);
        e.Handled = invalid;
        // If not invalid or null or empty, save value, else save 0
        _sectorDict[SelectedSector].Max = invalid || string.IsNullOrEmpty(e.Text) ? 0 : int.Parse(e.Text);
    }
    private void DisableSpaces(object sender, KeyEventArgs e)
    {
        // Don't allow key if is space
        e.Handled = e.Key == Key.Space;
    }
    private void SectorColor_TextChanged(object sender, TextChangedEventArgs e)
    {
        // If no sector selected (which should not trigger this anyway, here just to be extra safe)
        // or changed from code, don't do anything
        if (SelectedSector is null || !s_isUserInvoked) return;

        // If validation failed aka invalid color
        if (Validation.GetHasError((TextBox)sender))
        {
            _sectorDict[SelectedSector].Color = null;
            return;
        }
        _sectorDict[SelectedSector].ColorNoHash = Property.Color;
    }

}