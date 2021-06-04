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
using MaterialDesignThemes.Wpf;
using System.Threading;

namespace VotingPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DialogHost.OpenDialogCommand.Execute(null, dialogHost);
            Init();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connection.CloseAsync();
            isListening = false;
            Thread.Sleep(800);
            if (serial != null)
            {
                serial.Close();
                serial.Dispose();
            }
        }
    }
}
