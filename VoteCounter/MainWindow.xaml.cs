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

namespace VoteCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global variables
        private readonly PasswordDialog passwordDialog;
        private static string databasePath;
        private readonly Dialogs dialogs;
        private static readonly List<List<Candidate>> sections = new();
        // List of information about sections (each section is a list of candidates)
        private static List<Info> infos;
        private static SQLiteAsyncConnection connection;
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            if (!ShowOpenDatabaseDialog()) return;

            // Check if file can be read or not. Exit if can't be read or not exist
            try
            {
                using FileStream file = new(databasePath, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("File cơ sở dữ liệu chỉ đọc. Thiếu quyền admin.\n" +
                    "Vui lòng chạy lại chương trình với quyền admin hoặc\n" +
                    "chuyển file vào nơi có thể ghi được như Desktop.", "OK", () =>
                    {
                        Close();
                    });
                return;
            }

            passwordDialog.Show();
        }

        /// <summary>
        /// Show Windows's default open file dialog to select database file, then save path to databasePath
        /// </summary>
        /// <returns>True if file if selected, else</returns>
        private bool ShowOpenDatabaseDialog()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Database file (*.db)|*.db",
                Multiselect = false,
                Title = "Chọn tệp cơ sở dữ liệu",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                databasePath = openFileDialog.FileName;
                return true;
            }
            else
            {
                Close();
                return false;
            }
        }
        /// <summary>
        /// Click handler for the Password Dialog button.
        /// </summary>
        private async void PasswordDialogButton_Click(object sender, RoutedEventArgs e)
        {
            dialogs.CloseDialog();
            dialogs.ShowLoadingDialog();

            // Create new SQLite database connection
            SQLiteConnectionString options = new(databasePath, storeDateTimeAsTicks: true, passwordDialog.Password);
            connection = new SQLiteAsyncConnection(options);

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

            // If thing goes well aka correct password, run the init code
            Init();
        }


        ///***********************************************************///
        /// Loading section
        ///***********************************************************///

        /// <summary>
        /// Try to read from database and validate content
        /// </summary>
        private async void Init()
        {
            // Try to load database
            try { await LoadDatabase(); }
            catch (Exception) { InvalidDatabase(); return; }

            if (ValidateDatabase() == false)
            {
                InvalidDatabase();
                return;
            }

            PopulateVoteUI();
            dialogs.CloseDialog();
        }
        /// <summary>
        /// Load database into infos and sections Lists
        /// </summary>
        /// <returns>An awaitable Task that do the work</returns>
        #region Database loading and validating
        private static async Task LoadDatabase()
        {
            string query = $"SELECT * FROM Info";
            infos = await connection.QueryAsync<Info>(query);
            query = $"SELECT * FROM \"";
            foreach (Info info in infos)
            {
                sections.Add(await connection.QueryAsync<Candidate>(query + info.Section + "\""));
            }
        }
        private void InvalidDatabase()
        {
            dialogs.CloseDialog();
            dialogs.ShowTextDialog("Cơ sở dữ liệu không hợp lệ, vui lòng kiểm tra lại.", "Đóng", () =>
            {
                Close();
            });
        }
        private static bool ValidateDatabase()
        {
            foreach (Info info in infos)
            {
                if (!info.IsValid) return false;
            }
            foreach (List<Candidate> sectionList in sections)
            {
                foreach (Candidate section in sectionList)
                {
                    if (!section.IsValid) return false;
                }
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Use loaded database lists to populate vote UI
        /// </summary>
        private void PopulateVoteUI()
        {
            int index = 0;
            foreach (Info info in infos)
            {
                  
            }
        }
    }
}
