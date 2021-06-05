using MaterialDesignThemes.Wpf.Transitions;
using SQLite;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VotingPC
{
    public partial class MainWindow : Window
    {
        private static SQLiteAsyncConnection connection;
        private static SerialPort serial;
        private static bool isListening = true;
        private static readonly List<List<Scale>> scales = new();
        private static List<Info> infos;
        private static readonly List<StackPanel> stacks = new();
        private static int currentScaleIndex;

        #region Functional methods
        private async void Init()
        {
            SQLiteConnectionString options = new(".\\database.db", storeDateTimeAsTicks: true, key: "12345678");
            connection = new SQLiteAsyncConnection(options);
            await Task.Run(GetArduinoCOMPort);
            if (serial == null)
            {
                _ = MessageBox.Show("Không tìm thấy thiết bị Arduino. Vui lòng gọi kỹ thuật viên.", "Lỗi", MessageBoxButton.OK,
                MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Close();
                return;
            }
            await LoadDatabase();
            PopulateVoteUI();
            CloseDialog();
            WaitForSignal();
        }
        private async void WaitForSignal()
        {
            if (serial == null) return;
            serial.DiscardInBuffer();
            while (isListening)
            {
                while (isListening && serial.BytesToRead < 1)
                {
                    await Task.Delay(100);
                }
                if (serial.IsOpen && (serial.ReadByte() == 'D'))
                {
                    serial.Write("X");
                    NextPage();
                    ((RadioButton)slide2.votePanel.Children[0]).IsChecked = true;
                    break;
                }
            }
        }
        private void GetArduinoCOMPort()
        {
            string[] COMPorts = SerialPort.GetPortNames();
            foreach (string COMPort in COMPorts)
            {
                SerialPort serialPort = new(COMPort, 115200);
                serialPort.Open();
                serialPort.Write("R");
                for (int i = 0; i < 10; i++)
                {
                    if (serialPort.BytesToRead == 0)
                    {
                        Thread.Sleep(500);
                        serialPort.Write("R");
                    }
                    else break;
                }
                if (serialPort.BytesToRead == 0)
                {
                    serialPort.Dispose();
                    continue;
                }
                int response = serialPort.ReadByte();
                if (response == 'K')
                {
                    serial = serialPort;
                    return;
                }
                serialPort.Dispose();
            }
        }
        private static async Task LoadDatabase()
        {
            string query = $"SELECT * FROM Info";
            infos = await connection.QueryAsync<Info>(query);
            query = $"SELECT * FROM ";
            foreach (Info info in infos)
            {
                scales.Add(await connection.QueryAsync<Scale>(query + info.Scale));
            }
        }
        private void PopulateVoteUI()
        {
            int index = 0;
            foreach (Info info in infos)
            {
                RadioButton button = new();
                button.Style = (Style)Application.Current.Resources["MaterialDesignTabRadioButton"];
                button.Margin = new Thickness(4);
                button.Content = info.Scale;
                button.IsChecked = false;
                button.Checked += ChangeSlide_Checked;
                slide2.votePanel.Children.Add(button);

                StackPanel stackPanel = new();
                stackPanel.Margin = new Thickness(80, 30, 80, 50);
                stackPanel.Visibility = Visibility.Collapsed;

                currentScaleIndex = index;
                foreach (var item in scales[index])
                {
                    CheckBox checkBox = new();

                    item.Gender = item.Gender.Trim();
                    if (item.Gender == "Bà")
                    {
                        checkBox.Content = "Bà      " + item.Name;
                    }
                    else if (item.Gender == "Ông")
                    {
                        checkBox.Content = "Ông   " + item.Name;
                    }
                    else
                    {
                        checkBox.Content = item.Gender + "   " + item.Name;
                    }

                    checkBox.Checked += CheckBox_Checked;
                    checkBox.IsChecked = true;
                    checkBox.Unchecked += CheckBox_Unchecked;
                    checkBox.Margin = new Thickness(0, 30, 0, 30);
                    checkBox.HorizontalAlignment = HorizontalAlignment.Left;
                    checkBox.Style = (Style)Application.Current.Resources["MaterialDesignFilterChipPrimaryOutlineCheckBox"];
                    checkBox.RenderTransform = new ScaleTransform(2, 2);

                    stackPanel.Children.Add(checkBox);
                }
                stacks.Add(stackPanel);
                slide2.voteStack.Children.Add(stacks[index]);
                index++;
            }
            currentScaleIndex = 0;
            slide2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(infos[0].Color));
            slide2.submitButton.Click += SubmitButton_Click;
        }
        private static void ResetCheckboxes()
        {
            foreach (StackPanel stack in stacks)
            {
                foreach (CheckBox checkBox in stack.Children)
                {
                    if (!(bool)checkBox.IsChecked)
                    {
                        checkBox.IsChecked = true;
                    }
                }
            }
        }
        #endregion


        #region UI methods
        // Interaction for slides. Bad practice but eh, the app is extremely simple, so...
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            infos[currentScaleIndex].TotalVoted++;
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            infos[currentScaleIndex].TotalVoted--;
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            bool validate = true; string errors = "";
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].TotalVoted > infos[i].Max)
                {
                    // TODO: Add error to error box (somehow)
                    validate = false;
                    errors += "Số đại biểu được bầu quá số lượng tối đa tại Phiếu bầu " + infos[i].Title + "\n";
                    continue;
                }
                for (int j = 0; j < scales[i].Count; j++)
                {
                    if ((bool)((CheckBox)stacks[i].Children[j]).IsChecked)
                    {
                        scales[i][j].Votes++;
                    }
                }
            }

            if (!validate)
            {
                ShowTextDialog(errors, "Trở lại");
            }
            else
            {
                ShowLoadingDialog();
                for (int i = 0; i < infos.Count; i++)
                {
                    for (int j = 0; j < scales[i].Count; j++)
                    {
                        string name = scales[i][j].Name.Replace("\'", "\'\'");
                        // Save stuff to db file
                        string query = $"UPDATE {infos[i].Scale} SET Votes = {scales[i][j].Votes} WHERE Name = '{name}';";
                        await connection.ExecuteAsync(query);
                    }
                }
                CloseDialog();

                ShowTextDialog("Đã nộp phiếu bầu. Chúc một ngày tốt lành.", "Đóng", () =>
                {
                    PreviousPage();
                    ResetCheckboxes();
                    WaitForSignal();
                });
            }
        }
        private void ChangeSlide_Checked(object sender, RoutedEventArgs e)
        {
            currentScaleIndex = slide2.votePanel.Children.IndexOf((UIElement)sender);
            for (int i = 0; i < stacks.Count; i++)
            {
                stacks[i].Visibility = Visibility.Collapsed;
            }
            stacks[currentScaleIndex].Visibility = Visibility.Visible;
            slide2.title.Text = "Đại biểu " + infos[currentScaleIndex].Title + " " + infos[currentScaleIndex].Year;
            slide2.caption.Text = "Chọn tối đa " + infos[currentScaleIndex].Max + " Trần Dần";
            slide2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(infos[currentScaleIndex].Color));
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
        #endregion
    }
}
