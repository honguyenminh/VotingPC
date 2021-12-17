using VotingPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SQLite;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using AsyncDialog;

namespace VoteCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Global variables
        private static string[] s_databasePath;
        private readonly AsyncDialogManager _dialogs;
        private static readonly Dictionary<string, List<Candidate>> s_sections = new();
        // List of information about sections (each sector is a list of candidates)
        private static Dictionary<string, Info> s_infos;
        #endregion

        public MainWindow()
        {
            SQLitePCL.Batteries_V2.Init();
            InitializeComponent();

            // Init Dialogs classes for MaterialDesign dialogs
            _dialogs = new AsyncDialogManager(dialogHost);
            
            // Set click event handlers for buttons in slides
            slideLanding.SingleFileButton.Click += SingleFileButton_Click;
            slideLanding.MultipleFileButton.Click += MultipleFileButton_Click;
            slideLanding.FolderButton.Click += FolderButton_Click;
            slideDetail.backButton.Click += (_, _) => PreviousPage();
        }

        // Button click events
        private async void SingleFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            if (!ShowOpenDatabaseFileDialog()) return;
            await ParseDbFiles();
        }
        private async void MultipleFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Allow multiple file, then open file dialog
            if (!ShowOpenDatabaseFileDialog(multiFile: true)) return;

            await ParseDbFiles();
        }
        private async void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new()
            {
                Description = @"Chọn thư mục chứa file .db",
                UseDescriptionForTitle = true
            };

            if (!(bool)dialog.ShowDialog()) return;

            string[] filePaths = Directory.GetFiles(dialog.SelectedPath, "*.db", SearchOption.AllDirectories);

            if (filePaths.Length == 0)
            {
                await _dialogs.ShowTextDialog("Không tìm thấy file cơ sở dữ liệu",
                    "Không tìm thấy file .db trong thư mục đã chọn.\n" 
                    + "Vui lòng kiểm tra lại.");
                return;
            }
            s_databasePath = filePaths;

            await ParseDbFiles();
        }

        /// <summary>
        /// Show Windows's default open file dialog to select database file(s), then save paths to databasePath
        /// </summary>
        /// <param name="multiFile">Allow multiple file or not</param>
        /// <returns>True if file(s) if selected, else false</returns>
        private static bool ShowOpenDatabaseFileDialog(bool multiFile = false)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Database file (*.db)|*.db",
                Title = "Chọn tệp cơ sở dữ liệu",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Multiselect = multiFile
            };

            if (openFileDialog.ShowDialog() != true) return false;
            s_databasePath = openFileDialog.FileNames;
            return true;
        }

        /// <summary>
        /// Show PasswordDialog to get password from user
        /// </summary>
        /// <returns><see langword="null"/> if user cancelled, else return the entered password as string</returns>
        private async Task<string> GetPassword()
        {
            string password;
            while (true)
            {
                password = await _dialogs.ShowPasswordDialog(
                    "Nhập mật khẩu cơ sở dữ liệu:",
                    "Mật khẩu",
                    "Để trống nếu không có mật khẩu",
                    "HOÀN TẤT",
                    "HỦY"
                );
                // User cancelled
                if (password is null)
                {
                    return null;
                }
                _dialogs.ShowLoadingDialog();
                // Create new SQLite database connection for each file
                SQLiteConnectionString options = new(s_databasePath[0], true, password);
                SQLiteAsyncConnection connection = new(options);

                // Check password
                try
                {
                    // This will try to query the SQLite Schema Database
                    // if the key is correct then no error is raised
                    _ = await connection.QueryAsync<int>("SELECT count(*) FROM sqlite_master");
                    await connection.CloseAsync();
                    break;
                }
                catch (SQLiteException) // Wrong password
                {
                    _dialogs.CloseDialog();
                    await connection.CloseAsync();
                }
            }
            return password;
        }
        private async Task ParseDbFiles()
        {
            string password = await GetPassword();
            if (password is null) return;

            foreach (string database in s_databasePath)
            {
                // Check if file can be read or not. Exit if can't be read or not exist
                try
                {
                    await using FileStream file = new(database, FileMode.Open, FileAccess.Read);
                }
                catch
                {
                    _dialogs.CloseDialog();
                    await _dialogs.ShowTextDialog(
                        title: "Không đủ quyền đọc file " + database,
                        text: "Vui lòng chạy lại chương trình với quyền admin hoặc\n" +
                        "chuyển file vào nơi có thể đọc được như Desktop."
                    );
                    return;
                }

                // Create new SQLite database connection for each file
                SQLiteConnectionString options = new(database, true, password);
                SQLiteAsyncConnection connection = new(options);

                // Check password again, just to be sure
                try
                {
                    _ = await connection.QueryAsync<int>("SELECT count(*) FROM sqlite_master");
                }
                catch (SQLiteException) // Wrong password
                {
                    _dialogs.CloseDialog();
                    await connection.CloseAsync();
                    // Just throw tbh
                    // TODO: add option to enter custom password
                    await _dialogs.ShowTextDialog(
                        title: "Sai mật khẩu tại file " + database,
                        text: "Vui lòng kiểm tra lại mật khẩu các file, đảm bảo mật khẩu trùng nhau"
                    );
                    return;
                }

                // Parse data from database
                try
                {
                    // Parse Info table about sections in current file
                    string query = $"SELECT * FROM Info";
                    List<Info> currentFileInfos = await connection.QueryAsync<Info>(query);
                    // Validate the parsed file
                    if (ValidateData(currentFileInfos) == false) throw new InvalidDataException();
                    // If infos is not read before, just save the object
                    if (s_infos == null)
                        s_infos = currentFileInfos.ToDictionary(x => x.Sector, x => x);
                    // Merge two infos if infos is read already
                    else
                    {
                        foreach (Info info in currentFileInfos)
                        {
                            if (s_infos.ContainsKey(info.Sector)) continue;
                            s_infos.Add(info.Sector, info);
                        }
                    }
                    
                    query = "SELECT * FROM \"";
                    foreach (Info sector in currentFileInfos)
                    {
                        List<Candidate> candidateList = await connection.QueryAsync<Candidate>(query + sector.Sector + "\"");
                        if (ValidateData(candidateList) == false) throw new InvalidDataException();

                        FindMaxVote(candidateList);

                        // If sector not yet saved, save new sector into sections collection
                        if (!s_sections.ContainsKey(sector.Sector))
                        {
                            s_sections.Add(sector.Sector, candidateList);
                            continue;
                        }

                        // Else merge with current file's sector
                        Dictionary<string, Candidate> candidateDict = candidateList.ToDictionary(x => x.Name, x => x);
                        foreach (Candidate candidate in s_sections[sector.Sector])
                        {
                            candidate.Votes += candidateDict[candidate.Name].Votes;
                            candidate.TotalWinningPlaces += candidateDict[candidate.Name].TotalWinningPlaces;
                        }
                    }
                }
                catch
                {
                    // TODO: Add detailed error report if possible
                    _dialogs.CloseDialog();
                    await _dialogs.ShowTextDialog("Cơ sở dữ liệu không hợp lệ, vui lòng kiểm tra lại.", buttonLabel: "Đóng");
                    s_infos.Clear();
                    s_sections.Clear();
                    return;
                }
            }

            // The line below equals to await Task.Run(PopulateUI); but WPF is stupid so...
            Application.Current.Dispatcher.Invoke(PopulateUi);
            _dialogs.CloseDialog();
            NextPage();
        }


        ///***********************************************************///
        /// Loading section
        ///***********************************************************///
        #region Database validating
        private static bool ValidateData(List<Info> infoList)
        {
            foreach (Info info in infoList)
            {
                if (!info.IsValid) return false;
            }
            return true;
        }
        private static bool ValidateData(List<Candidate> candidateList)
        {
            foreach (Candidate candidate in candidateList)
            {
                if (!candidate.IsValid) return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Use loaded database lists to populate vote UI
        /// </summary>
        private void PopulateUi()
        {
            bool left = true;
            foreach (var (_, value) in s_infos)
            {
                // Sort the candidate list
                SortByHighestVotes(s_sections[value.Sector]);

                DisplayCard displayCard = new(value, s_sections[value.Sector]);
                if (left)
                {
                    displayCard.Margin = new(0, 16, 56, 16);
                    left = false;
                }
                else
                {
                    displayCard.Margin = new(56, 16, 0, 16);
                    left = true;
                }


                displayCard.Click += (sender, _) => {
                    slideDetail.ChangeData(((DisplayCard)sender).SectionInfo, ((DisplayCard)sender).Candidates);
                    NextPage();
                };
                _ = slideOverview.OverviewStackPanel.Children.Add(displayCard);
            }
        }

        /// <summary>
        /// Sort the given list of Candidate by highest votes to lowest votes
        /// </summary>
        /// <param name="candidates">List of candidates to sort</param>
        private static void SortByHighestVotes(in List<Candidate> candidates)
        {
            // Compare function that compares the votes inside candidates to sort candidates
            int Comparison(Candidate x, Candidate y) => (int) y.Votes - (int) x.Votes;
            candidates.Sort(Comparison);
        }
        private static void FindMaxVote(in List<Candidate> candidates)
        {
            if (candidates.Count == 0) throw new ArgumentException("Empty candidate list as parameter");
            long max = candidates[0].Votes;
            List<int> winningCandidates = new() {0};
            for (int i = 1; i < candidates.Count; i++)
            {
                if (candidates[i].Votes < max) continue;
                if (candidates[i].Votes > max)
                {
                    winningCandidates.Clear();
                    max = candidates[i].Votes;
                }
                winningCandidates.Add(i);
            }

            foreach (int index in winningCandidates)
            {
                candidates[index].TotalWinningPlaces++;
            }
        }

        #region Transitioner methods
        /// <summary>
        /// Move to next Slide
        /// </summary>
        private void NextPage()
        {
            // 0 is argument, meaning no arg
            // Pass object as second argument
            Transitioner.MoveNextCommand.Execute(null, TransitionerObj);
        }
        /// <summary>
        /// Move to previous Slide
        /// </summary>
        private void PreviousPage()
        {
            Transitioner.MovePreviousCommand.Execute(null, TransitionerObj);
        }
        #endregion
    }
}
