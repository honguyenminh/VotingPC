using System.Threading;
using System.Windows;

namespace VotingPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PasswordDialog passwordDialog;
        public MainWindow()
        {
            InitializeComponent();
            passwordDialog = new("Nhập mật khẩu cơ sở dữ liệu:", "Mật khẩu không chính xác, vui lòng nhập lại:", "Hoàn tất", PasswordDialogButton_Click);
            ShowPasswordDialog();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connection != null) _ = connection.CloseAsync();
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
