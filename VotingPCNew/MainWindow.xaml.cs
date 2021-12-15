using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using AsyncDialog;
using MaterialDesignThemes.Wpf.Transitions;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using SQLite;
using SQLitePCL;
using VotingPCNew.Scanner;

// using System.Collections.Generic;
// using System.Windows.Controls;
// using System.Windows.Data;
// using System.Windows.Documents;
// using System.Windows.Input;
// using System.Windows.Media;
// using System.Windows.Media.Imaging;
// using System.Windows.Navigation;
// using System.Windows.Shapes;

namespace VotingPCNew
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int DialogDelayDuration = 400;
        private const string ConfigPath = "config.json";
        private readonly string _configParseError;

        private readonly AsyncDialogManager _dialogs;
        private readonly ScannerManager _scanner;

        public MainWindow()
        {
            Batteries_V2.Init();
            InitializeComponent();
            _dialogs = new AsyncDialogManager(dialogHost)
            {
                ScaleFactor = 1.5
            };

            // Init scanner manager
            ScannerSignalTable signalTable = new()
            {
                Acknowledgement = 'N',
                Receive = new ReceiveSignalTable
                {
                    FingerFound = 'F'
                },
                Send = new SendSignalTable
                {
                    StartScanning = 'S',
                    AcknowledgedFinger = 'K',
                    Close = 'C'
                }
            };
            _scanner = new ScannerManager(signalTable);

            // Read config.json config file
            // TODO: fix error handling for json
            try
            {
                string configFile = File.ReadAllText(ConfigPath);
                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true
                };
                var config = JsonSerializer.Deserialize<Config>(configFile, options);

                slide1.TitleConfig = config.Title;
                slide1.TopHeaderConfig = config.Header;
                slide1.TopSubheaderConfig = config.Subheader;
                // Replace logo with custom image in app folder if exists and is valid
                if (File.Exists(config.IconPath))
                {
                    slide1.IconPath = config.IconPath;
                }
                else _configParseError = @"Không tìm thấy file logo. Kiểm tra lại đường dẫn.";
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
                await _dialogs.ShowTextDialog(_configParseError, @"Lỗi file config.json");
                Close();
                return;
            }

            string databasePath = ShowOpenDatabaseDialog();
            if (databasePath == null)
            {
                Close();
                return;
            }

            bool saveToMultipleFile = await _dialogs.ShowConfirmTextDialog
            (
                title: @"Chọn kiểu lưu",
                text: @"Lưu kết quả vào file ban đầu hay tách các cấp thành nhiều file?",
                leftButtonLabel: @"FILE BAN ĐẦU",
                rightButtonLabel: @"NHIỀU FILE"
            );

            _dialogs.ShowLoadingDialog();
            // Check if file can be written to or not. Exit if read-only
            bool isReadOnly = false;
            if (!saveToMultipleFile)
            {
                try
                {
                    await using FileStream file = new(databasePath, FileMode.Open, FileAccess.ReadWrite);
                }
                catch
                {
                    isReadOnly = true;
                }
            }
            else
            {
                // Show open folder dialog
                VistaFolderBrowserDialog dialog = new()
                {
                    Description = @"Chọn thư mục chứa file cơ sở dữ liệu xuất ra",
                    UseDescriptionForTitle = true
                };
                // If cancelled, exit the app
                if (!(bool) dialog.ShowDialog())
                {
                    Close();
                    return;
                }

                // Check if folder can be written to or not
                DirectoryInfo directoryInfo = new(dialog.SelectedPath);
                isReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            }

            if (isReadOnly)
            {
                _dialogs.CloseDialog();
                await Task.Delay(DialogDelayDuration);
                await _dialogs.ShowTextDialog(
                    title: "File/thư mục chỉ đọc",
                    text: @"Thiếu quyền Admin hoặc phân quyền sai.\n"
                          + @"Vui lòng chạy lại chương trình với quyền Admin, sửa quyền truy cập file\n"
                          + @"hoặc chuyển file/thư mục vào nơi có thể ghi được như Desktop.");
                Close();
                return;
            }

            _dialogs.CloseDialog();
            await Task.Delay(DialogDelayDuration);

            string password;
            SQLiteAsyncConnection connection;
            while (true)
            {
                // Get password
                password = await _dialogs.ShowPasswordDialog
                (
                    @"Nhập mật khẩu cơ sở dữ liệu",
                    @"Mật khẩu",
                    @"Để trống nếu không có mật khẩu",
                    cancelButtonLabel: @"THOÁT ỨNG DỤNG"
                );
                // Quit app if user cancelled
                if (password is null)
                {
                    Close();
                    return;
                }

                // Try to open connection to db
                _dialogs.ShowLoadingDialog();
                connection = await OpenDatabaseAsync(databasePath, password);
                if (connection is null) // Wrong password
                {
                    await Task.Delay(3000); // Avoid brute-force
                    _dialogs.CloseDialog();
                    await Task.Delay(DialogDelayDuration);
                    await _dialogs.ShowTextDialog(@"Mật khẩu sai, vui lòng nhập lại.");
                }
                else break;
            }

            _dialogs.CloseDialog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }

        /// <summary>
        ///     Show a dialog to select database file
        /// </summary>
        /// <returns>A path to selected file, null if not selected or canceled</returns>
        private static string ShowOpenDatabaseDialog(string title = @"Chọn tệp cơ sở dữ liệu")
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
        ///     Move to next Slide
        /// </summary>
        private void NextPage()
        {
            Transitioner.MoveNextCommand.Execute(null, transitionerObj);
        }

        /// <summary>
        ///     Move to previous Slide
        /// </summary>
        private void PreviousPage()
        {
            Transitioner.MovePreviousCommand.Execute(null, transitionerObj);
        }
    }
}