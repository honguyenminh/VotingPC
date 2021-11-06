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
                        Uri fullPath = new(filePath, UriKind.Absolute);
                        BitmapImage image = new(fullPath);
                        slide1.IconPath = filePath;
                        break;
                    }
                    catch (Exception) { }
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

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
