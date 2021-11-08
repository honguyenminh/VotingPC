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

            // Check if file can be written to or not. Exit if read-only
            // TODO: check this only after asking for multi-file
            try
            {
                using FileStream file = new(databasePath, FileMode.Open, FileAccess.ReadWrite);
            }
            catch
            {
                await dialogs.ShowTextDialog(title: "File cơ sở dữ liệu chỉ đọc.",
                    text: "Thiếu quyền Admin.\n" +
                    "Vui lòng chạy lại chương trình với quyền Admin hoặc\n" +
                    "chuyển file vào nơi có thể ghi được như Desktop.");
                Close();
                return;
            }
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

            if (openFileDialog.ShowDialog() == true) return openFileDialog.FileName;
            else return null;
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
