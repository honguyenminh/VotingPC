using MaterialDesignThemes.Wpf.Transitions;
using Microsoft.Win32;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VotingPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PasswordDialog passwordDialog;
        private string databasePath;
        private string folderPath;
        private readonly Dialogs dialogs;
        private readonly string[] supportedIconExt = { ".png", ".jpg", ".jpeg", ".jpe", ".gif", ".ico", ".tiff", ".bmp" };
        public MainWindow()
        {
            SQLitePCL.Batteries_V2.Init();
            InitializeComponent();

            // Replace logo with custom image in app folder if exists and is valid
            string filePathWithoutExt = AppDomain.CurrentDomain.BaseDirectory + "\\logo";
            foreach (string extension in supportedIconExt)
            {
                string filePath = filePathWithoutExt + extension;
                if (File.Exists(filePath))
                {
                    try
                    {
                        BitmapImage image = new(new Uri(filePath, UriKind.Absolute));
                        slide1.logoImage.Source = image;
                        break;
                    }
                    catch (Exception) { }
                }
            }

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

            // Check if file can be written to or not. Exit if read-only
            try
            {
                using FileStream file = new(databasePath, FileMode.Open, FileAccess.ReadWrite);
            }
            catch
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("File cơ sở dữ liệu chỉ đọc. Thiếu quyền Admin.\n" +
                    "Vui lòng chạy lại chương trình với quyền Admin hoặc\n" +
                    "chuyển file vào nơi có thể ghi được như Desktop.", "OK", Close);
                return;
            }

            // Pop-up dialog to select file save method
            // After clicked, show password dialog
            dialogs.Show2ChoiceDialog("Lưu kết quả vào file ban đầu hay tách\ncác cấp thành nhiều file?", "File ban đầu", "Nhiều file",
                async (sender, e) => { saveToMultipleFile = false; dialogs.CloseDialog(); await Task.Delay(400); passwordDialog.Show(); },
                MultipleFileButton_Click);
        }
        private async void MultipleFileButton_Click(object sender, RoutedEventArgs e)
        {
            saveToMultipleFile = true;
            dialogs.CloseDialog();

            // Show open folder dialog
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new()
            {
                Description = "Chọn thư mục chứa file cơ sở dữ liệu xuất ra",
                UseDescriptionForTitle = true
            };
            if (!(bool)dialog.ShowDialog()) { Close(); return; }

            dialogs.ShowLoadingDialog();
            // Check if folder can be written to or not
            var directoryInfo = new DirectoryInfo(dialog.SelectedPath);
            bool isReadonly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            if (isReadonly)
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("Thư mục chỉ đọc. Thiếu quyền Admin.\n" +
                    "Vui lòng chạy lại chương trình với quyền Admin hoặc\n" +
                    "chọn thư mục khác có thể ghi được.", "OK", Close);
                return;
            }

            folderPath = dialog.SelectedPath;
            dialogs.CloseDialog();
            await Task.Delay(400);
            passwordDialog.Show();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (SQLite.SQLiteAsyncConnection connection in connectionList)
                _ = connection?.CloseAsync();
            
            isListening = false;
            if (serial != null)
            {
                serial.Write("C"); // App closed signal
                serial.Close();
                serial.Dispose();
            }
        }

        // Transition methods
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
    }
}
