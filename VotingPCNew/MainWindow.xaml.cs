using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using MaterialDesignThemes.Wpf.Transitions;

namespace VotingPCNew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AsyncDialog.AsyncDialog dialogs;
        private readonly string[] supportedIconExt = { ".png", ".jpg", ".jpeg", ".jpe", ".gif", ".ico", ".tiff", ".bmp" };
        public MainWindow()
        {
            SQLitePCL.Batteries_V2.Init();
            InitializeComponent();
            dialogs = new(dialogHost);

            // Replace logo with custom image in app folder if exists and is valid
            string filePathWithoutExt = AppDomain.CurrentDomain.BaseDirectory + "\\logo";
            foreach (string extension in supportedIconExt)
            {
                string filePath = filePathWithoutExt + extension;
                if (File.Exists(filePath))
                {
                    try
                    {
                        Uri fullPath = new(filePath, UriKind.Absolute);
                        BitmapImage image = new(fullPath);
                        slide1.IconPath = filePath;
                        break;
                    }
                    catch (Exception) { }
                }
            }

            // TODO: add json config file parsing
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string databasePath = ShowOpenDatabaseDialog();
            if (databasePath is null)
            {
                Close(); return;
            }

            bool saveToMultipleFile = await dialogs.ShowConfirmTextDialog(
                title: "Chọn kiểu lưu",
                text: "Lưu kết quả vào file ban đầu hay tách các cấp thành nhiều file?",
                leftButtonLabel: "FILE BAN ĐẦU",
                rightButtonLabel: "NHIỀU FILE");

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
                await Task.Delay(400);
                await dialogs.ShowTextDialog(
                    title: "File/thư mục chỉ đọc",
                    text: "Thiếu quyền Admin hoặc phân quyền sai.\n" +
                    "Vui lòng chạy lại chương trình với quyền Admin hoặc\n" +
                    "chuyển file/thư mục vào nơi có thể ghi được như Desktop.");
                Close(); return;
            }

            dialogs.CloseDialog();
            await Task.Delay(400);
            // TODO: show password dialog here
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
