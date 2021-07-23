using VotingPC;
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
using System.IO;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using MaterialDesignThemes.Wpf.Transitions;

namespace VoteCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global variables
        private readonly PasswordDialog passwordDialog;
        private static string[] databasePath;
        private readonly Dialogs dialogs;
        private static readonly Dictionary<string, List<Candidate>> sections = new();
        // List of information about sections (each section is a list of candidates)
        private static Dictionary<string, Info> infos;
        #endregion

        public MainWindow()
        {
            SQLitePCL.Batteries_V2.Init();
            InitializeComponent();

            // Init Dialogs classes for MaterialDesign dialogs
            dialogs = new(dialogHost);
            passwordDialog = new(dialogHost,
                "Nhập mật khẩu cơ sở dữ liệu:",
                "Mật khẩu không chính xác hoặc cơ sở dữ liệu không hợp lệ!",
                "Hoàn tất", PasswordDialogButton_Click);

            // Set click event handlers for buttons in slide 1
            SlideLanding.SingleFileButton.Click += SingleFileButton_Click;
            SlideLanding.MultipleFileButton.Click += MultipleFileButton_Click;
            SlideLanding.FolderButton.Click += FolderButton_Click;
        }

        // Button click events
        private void SingleFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            if (!ShowOpenDatabaseDialog()) return;

            passwordDialog.Show();
        }
        private void MultipleFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            if (!ShowOpenDatabaseDialog(multiFile: true)) return;

            passwordDialog.Show();
        }
        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Show Windows's default open file dialog to select database file(s), then save paths to databasePath
        /// </summary>
        /// <param name="multiFile">Allow multiple file or not</param>
        /// <returns>True if file(s) if selected, else false</returns>
        private static bool ShowOpenDatabaseDialog(bool multiFile = false)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Database file (*.db)|*.db",
                Title = "Chọn tệp cơ sở dữ liệu",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            openFileDialog.Multiselect = multiFile;

            if (openFileDialog.ShowDialog() == true)
            {
                databasePath = openFileDialog.FileNames;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Click handler for the Password Dialog button.
        /// </summary>
        private async void PasswordDialogButton_Click(object sender, RoutedEventArgs e)
        {
            dialogs.CloseDialog();
            dialogs.ShowLoadingDialog();

            foreach (string database in databasePath)
            {
                // Check if file can be read or not. Exit if can't be read or not exist
                try
                {
                    using FileStream file = new(database, FileMode.Open, FileAccess.Read);
                }
                catch
                {
                    dialogs.CloseDialog();
                    dialogs.ShowTextDialog("Không đủ quyền đọc file đã chọn.\n" +
                        "Vui lòng chạy lại chương trình với quyền admin hoặc\n" +
                        "chuyển file vào nơi có thể đọc được như Desktop.", "OK", () =>
                        {
                            Close();
                        });
                    return;
                }

                // Create new SQLite database connection for each file
                SQLiteConnectionString options = new(database, storeDateTimeAsTicks: true, passwordDialog.Password);
                SQLiteAsyncConnection connection = new(options);

                // Check password
                try
                {
                    //This will try to query the SQLite Schema Database, if the key is correct then no error is raised
                    _ = await connection.QueryAsync<int>("SELECT count(*) FROM sqlite_master");
                }
                catch (SQLiteException) // Wrong password
                {
                    dialogs.CloseDialog();
                    await connection.CloseAsync();
                    // Request password from user again, don't run init code
                    passwordDialog.Show(true);
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
                    if (infos == null)
                        infos = currentFileInfos.ToDictionary(x => x.Section, x => x);
                    // Merge two infos if infos is read already
                    else
                    {
                        foreach (Info info in currentFileInfos)
                        {
                            if (infos.ContainsKey(info.Section)) continue;
                            infos.Add(info.Section, info);
                        }
                    }

                    query = $"SELECT * FROM \"";
                    foreach (Info section in currentFileInfos)
                    {
                        List<Candidate> candidateList = await connection.QueryAsync<Candidate>(query + section.Section + "\"");
                        if (ValidateData(candidateList) == false) throw new InvalidDataException();

                        // If section not yet saved, save new section into sections collection
                        if (!sections.ContainsKey(section.Section))
                        {
                            sections.Add(section.Section, candidateList);
                            continue;
                        }

                        // Else merge with current file's section
                        Dictionary<string, Candidate> candidateDict = candidateList.ToDictionary(x => x.Name, x => x);
                        foreach (Candidate candidate in sections[section.Section])
                        {
                            candidate.Votes += candidateDict[candidate.Name].Votes;
                        }
                    }
                }
                catch
                {
                    // TODO: Add detailed error report if possible
                    dialogs.CloseDialog();
                    dialogs.ShowTextDialog("Cơ sở dữ liệu không hợp lệ, vui lòng kiểm tra lại.", "Đóng", () =>
                    {
                        Close();
                    });
                    return;
                }
            }

            // The line below equals to await Task.Run(PopulateUI); but WPF is stupid so...
            Application.Current.Dispatcher.Invoke(PopulateUI);
            dialogs.CloseDialog();
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
        private void PopulateUI()
        {
            bool left = true;
            foreach (var info in infos)
            {
                // Sort the candidate list
                SortByHighestVotes(sections[info.Value.Section]);

                DisplayCard displayCard = new(info.Value, sections[info.Value.Section]);
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

                // TODO: implement the detail slide here instead of this
                displayCard.Click += (sender, e) => { dialogs.ShowTextDialog("Hello, click được nhá.\nNhớ thêm cái slide detail đấy.", "OK"); };
                _ = SlideOverview.OverviewStackPanel.Children.Add(displayCard);
            }
        }

        /// <summary>
        /// Sort the given list of Candidate by highest votes to lowest votes
        /// </summary>
        /// <param name="candidates">List of candidates to sort</param>
        private static void SortByHighestVotes(List<Candidate> candidates)
        {
            // Compare function, hover over new() for more info on how this works
            Comparison<Candidate> comparison = new((x, y) => (int)y.Votes - (int)x.Votes);
            candidates.Sort(comparison);
        }

        #region Transitioner methods
        /// <summary>
        /// Move to next Slide
        /// </summary>
        private void NextPage()
        {
            // 0 is argument, meaning no arg
            // Pass object as second argument
            Transitioner.MoveNextCommand.Execute(0, TransitionerObj);
        }
        /// <summary>
        /// Move to previous Slide
        /// </summary>
        private void PreviousPage()
        {
            Transitioner.MovePreviousCommand.Execute(0, TransitionerObj);
        }
        #endregion
    }
}
