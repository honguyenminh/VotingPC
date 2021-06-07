using MaterialDesignThemes.Wpf.Transitions;
using SQLite;
using System.Collections.Generic;
using System.Globalization;
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
                // Add a button to scale chooser card on top left
                RadioButton button = new();
                button.Style = (Style)Application.Current.Resources["MaterialDesignTabRadioButton"];
                button.Margin = new Thickness(4);
                button.Content = info.Scale;
                button.IsChecked = false;
                button.Checked += ChangeSlide_Checked;
                _ = slide2.votePanel.Children.Add(button);

                // Add new page as StackPanel to the voteStack on right hand
                StackPanel stackPanel = new();
                stackPanel.Margin = new Thickness(72, 40, 72, 40);
                stackPanel.HorizontalAlignment = HorizontalAlignment.Left;

                currentScaleIndex = index;
                foreach (Scale item in scales[index])
                {
                    TextBlock content = new();
                    content.TextTrimming = TextTrimming.WordEllipsis;

                    item.Gender = item.Gender.Trim();
                    content.Text = item.Gender switch
                    {
                        "Bà" => "Bà      " + item.Name,
                        "Ông" => "Ông   " + item.Name,
                        _ => item.Gender + "   " + item.Name,
                    };

                    CheckBox checkBox = new()
                    {
                        Content = content,
                        Margin = new Thickness(0, 16, 0, 16),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Style = (Style)Application.Current.Resources["MaterialDesignFilterChipPrimaryOutlineCheckBox"],
                        LayoutTransform = new ScaleTransform(2, 2)
                    };
                    checkBox.Unchecked += CheckBox_Unchecked;
                    checkBox.Checked += CheckBox_Checked;
                    checkBox.IsChecked = true;

                    checkBox.Loaded += (sender, e) =>
                    {
                        if (GetTheoreticalSize(content).Width > content.RenderSize.Width)
                        {
                            TextBlock textBlock = new()
                            {
                                TextWrapping = TextWrapping.Wrap,
                                FontWeight = FontWeights.Normal,
                                FontSize = 20,
                                Text = content.Text
                            };
                            checkBox.ToolTip = textBlock;
                        }
                    };

                    _ = stackPanel.Children.Add(checkBox);
                }
                stacks.Add(stackPanel);
                index++;
            }
            //currentScaleIndex = 0;
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
        private static Size GetTheoreticalSize(TextBlock textBlock)
        {
            FormattedText formattedText = new(
                        textBlock.Text,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                        textBlock.FontSize,
                        Brushes.Black,
                        new NumberSubstitution(),
                        1);
            return new Size(formattedText.Width, formattedText.Height);
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
                        _ = await connection.ExecuteAsync(query);
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
            slide2.voteStack.Children.Clear();
            _ = slide2.voteStack.Children.Add(stacks[currentScaleIndex]);

            slide2.title.Text = "Đại biểu " + infos[currentScaleIndex].Title + " " + infos[currentScaleIndex].Year;
            slide2.caption.Text = "Chọn tối đa " + infos[currentScaleIndex].Max + " Trần Dần";
            slide2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(infos[currentScaleIndex].Color));
            slide2.voteCard.MinWidth = slide2.mainGrid.ColumnDefinitions[1].ActualWidth - 120;
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
