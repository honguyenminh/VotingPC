using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
using MaterialDesignThemes.Wpf.Transitions;
using SQLite;
using VotingPCNew.Scanner;

namespace VotingPCNew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DialogDelayDuration = 400;
        private const string ConfigPath = "config.json";

        private readonly AsyncDialog.AsyncDialog dialogs;
        private SQLiteAsyncConnection connection;
        private readonly string[] supportedIconExt = { ".png", ".jpg", ".jpeg", ".jpe", ".gif", ".ico", ".tiff", ".bmp" };
        private readonly ScannerManager scanner;
        private readonly string _configParseError;

        public MainWindow()
        {
            SQLitePCL.Batteries_V2.Init();
            InitializeComponent();
            dialogs = new(dialogHost);
            dialogs.ScaleFactor = 1.5;

            // Init scanner manager
            ScannerSignalTable signalTable = new()
            {
                Acknowledgement = 'N',
                Receive = new()
                {
                    FingerFound = 'F'
                },
                Send = new()
                {
                    StartScanning = 'S',
                    AcknowledgedFinger = 'K',
                    Close = 'C'
                }
            };
            scanner = new(signalTable);


            // Read config.json config file
            try
            {
                string configFile = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<Config>(configFile);

                //slide1.TitleConfig = config.title;
                slide1.TopHeaderConfig = config.header;
                slide1.TopSubheaderConfig = config.subheader;
                // Replace logo with custom image in app folder if exists and is valid
                if (File.Exists(config.iconPath))
                {
                    slide1.IconPath = config.iconPath;
                }
                else _configParseError = "Không tìm thấy file logo. Kiểm tra lại đường dẫn.";
            }
            catch (Exception e)
            {
                _configParseError = e.Message;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_configParseError != null)
            {
                await dialogs.ShowTextDialog(_configParseError, "Lỗi file config.json");
                Close(); return;
            }
            string databasePath = ShowOpenDatabaseDialog();
            if (databasePath is null) { Close(); return; }

            bool saveToMultipleFile = await dialogs.ShowConfirmTextDialog
            (
                title: "Chọn kiểu lưu",
                text: "Lưu kết quả vào file ban đầu hay tách các cấp thành nhiều file?",
                leftButtonLabel: "FILE BAN ĐẦU",
                rightButtonLabel: "NHIỀU FILE"
            );

            dialogs.ShowLoadingDialog();
            // Check if file can be written to or not. Exit if read-only
            bool isReadOnly = false;
            if (!saveToMultipleFile)
            {
                try
                {
                    using FileStream file = new(databasePath, FileMode.Open, FileAccess.ReadWrite);
                }
                catch { isReadOnly = true; }
            }
            else
            {
                // Show open folder dialog
                Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new()
                {
                    Description = "Chọn thư mục chứa file cơ sở dữ liệu xuất ra",
                    UseDescriptionForTitle = true
                };
                // If cancelled, exit the app
                if (!(bool)dialog.ShowDialog()) { Close(); return; }

                // Check if folder can be written to or not
                DirectoryInfo directoryInfo = new(dialog.SelectedPath);
                isReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            }
            if (isReadOnly)
            {
                dialogs.CloseDialog();
                await Task.Delay(DialogDelayDuration);
                await dialogs.ShowTextDialog(
                    title: "File/thư mục chỉ đọc",
                    text: "Thiếu quyền Admin hoặc phân quyền sai.\n" +
                    "Vui lòng chạy lại chương trình với quyền Admin, sửa quyền truy cập file\n" +
                    "hoặc chuyển file/thư mục vào nơi có thể ghi được như Desktop.");
                Close(); return;
            }

            dialogs.CloseDialog();
            await Task.Delay(DialogDelayDuration);

            string password;
            while (true)
            {
                // Get password
                password = await dialogs.ShowPasswordDialog
                (
                    title: "Nhập mật khẩu cơ sở dữ liệu",
                    passwordBoxLabel: "Mật khẩu",
                    passwordBoxHelperText: "Để trống nếu không có mật khẩu",
                    cancelButtonLabel: "THOÁT ỨNG DỤNG"
                );
                // Quit app if user cancelled
                if (password is null) { Close(); return; }

                // Try to open connection to db
                dialogs.ShowLoadingDialog();
                connection = await OpenDatabaseAsync(databasePath, password);
                if (connection is null) // Wrong password
                {
                    await Task.Delay(3000); // Avoid brute-force
                    dialogs.CloseDialog();
                    await Task.Delay(dialogDelayDuration);
                    await dialogs.ShowTextDialog("Mật khẩu sai, vui lòng nhập lại.");
                }
                else break;
            }
            connectionOpened = true;
            dialogs.CloseDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        /// <summary>
        /// Show a dialog to select database file
        /// </summary>
        /// <returns>A path to selected file, null if not selected or canceled</returns>
        private static string ShowOpenDatabaseDialog(string title = "Chọn tệp cơ sở dữ liệu")
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Database file (*.db)|*.db",
                Multiselect = false,
                Title = title,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        // Transition methods
        /// <summary>
        /// Move to next Slide
        /// </summary>
        private void NextPage()
        {
            Transitioner.MoveNextCommand.Execute(0, TransitionerObj);
        }
        /// <summary>
        /// Move to previous Slide
        /// </summary>
        private void PreviousPage()
        {
            Transitioner.MovePreviousCommand.Execute(0, TransitionerObj);
        }
    }
}
