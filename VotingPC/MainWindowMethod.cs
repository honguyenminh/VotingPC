using MaterialDesignThemes.Wpf.Transitions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// TODO: add invalid database check

namespace VotingPC
{
    public partial class MainWindow : Window
    {
        private static SQLiteAsyncConnection connection;
        private static SerialPort serial;
        private static bool isListening = true;
        private static readonly List<List<Scale>> scales = new(); // List of scales
        private static List<Info> infos; // List of information about scales
        // Contains vote stack panels, to preserve their state while user switch between scales
        private static readonly List<StackPanel> stacks = new();
        private static int currentScaleIndex;

        #region Functional methods
        /// <summary>
        /// Click handler for the Password Dialog button.
        /// </summary>
        private async void PasswordDialogButton_Click(object sender, RoutedEventArgs e)
        {
            dialogs.CloseDialog();
            dialogs.ShowLoadingDialog();

            // Create new SQLite database connection
            SQLiteConnectionString options = new(databasePath, storeDateTimeAsTicks: true, passwordDialog.Password);
            connection = new SQLiteAsyncConnection(options);

            try
            {
                //This will try to query the SQLite Schema Database, if the key is correct then no error is raised
                _ = await connection.QueryAsync<int>("SELECT count(*) FROM sqlite_master");
            }
            catch (SQLiteException) // Wrong password
            {
                dialogs.CloseDialog();
                await connection.CloseAsync();
                // Request password from user again, don't run init code
                passwordDialog.Show(true);
                return;
            }

            // If thing goes well aka correct password, run the init code
            Init();
        }
        /// <summary>
        /// Initialize application. With a loading dialog :))
        /// </summary>
        private async void Init()
        {
            // Get serial port
            try
            {
                serial = await Task.Run(ArduinoInteract.GetArduinoCOMPort);
            }
            catch (Exception e)
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("Lỗi tìm thiết bị Arduino. Vui lòng gọi kỹ thuật viên.\n" +
                    "Mã lỗi: " + e.Message, "OK", () =>
                    {
                        Close();
                    });
                return;
            }
            // When no serial port is found, dump error message box, then quit
            if (serial == null)
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("Không tìm thấy thiết bị Arduino. Vui lòng gọi kỹ thuật viên.", "OK", () =>
                {
                    Close();
                });
                return;
            }
            await LoadDatabase();
            PopulateVoteUI();
            dialogs.CloseDialog();  // Close loading dialog opened above
            WaitForSignal();        // Wait for serial signal from Arduino
        }
        /// <summary>
        /// Asynchronously check for signal from serial port in the background. NEVER await for this.
        /// </summary>
        private async void WaitForSignal()
        {
            if (serial == null) return;
            serial.Write("N"); // Send signal to start the reader
            while (isListening && serial.IsOpen)
            {
                while (isListening && serial.IsOpen && serial.BytesToRead < 1)
                {
                    await Task.Delay(100);
                }
                if (serial.IsOpen && (serial.ReadByte() == 'D'))
                {
                    serial.Write("X");
                    NextPage();
                    ((RadioButton)slide2.votePanel.Children[0]).IsChecked = true;
                    dialogs.ShowTextDialog("Đại biểu được bầu sẽ có dấu tích trước tên\n" +
                        "Nhấp chuột vào tên của người không tín nhiệm để bỏ dấu tích", "Đã rõ");
                    break;
                }
            }
        }
        /// <summary>
        /// Load database into infos and scales Lists
        /// </summary>
        /// <returns>An awaitable Task that do the work</returns>
        private static async Task LoadDatabase()
        {
            string query = $"SELECT * FROM Info";
            infos = await connection.QueryAsync<Info>(query);
            query = $"SELECT * FROM \"";
            foreach (Info info in infos)
            {
                scales.Add(await connection.QueryAsync<Scale>(query + info.Scale + "\""));
            }
        }
        /// <summary>
        /// Use loaded database lists to populate vote UI (Scale radio buttons, candidates, (sub)title)
        /// </summary>
        private void PopulateVoteUI()
        {
            int index = 0;
            foreach (Info info in infos)
            {
                // Add a button to scale chooser card on top left
                RadioButton button = new()
                {
                    Style = (Style)Application.Current.Resources["MaterialDesignTabRadioButton"],
                    Margin = new Thickness(4),
                    Content = info.Scale,
                    IsChecked = false
                };
                button.Checked += ChangeSlide_Checked;
                _ = slide2.votePanel.Children.Add(button);

                // Add new page as StackPanel to the voteStack on right hand
                StackPanel stackPanel = new()
                {
                    Margin = new Thickness(72, 40, 72, 40),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                currentScaleIndex = index;
                foreach (Scale item in scales[index])
                {
                    TextBlock content = new() { TextTrimming = TextTrimming.WordEllipsis };

                    item.Gender = item.Gender.Trim();
                    content.Text = item.Gender switch
                    {
                        "Bà" => "Bà      " + item.Name,
                        "Ông" => "Ông   " + item.Name,
                        _ => item.Gender + "   " + item.Name
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

                    // Add ToolTip if name is too long
                    checkBox.Loaded += (sender, e) =>
                    {
                        // TODO: Run this only once, this is not optimized, and will run on every slide change event.
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
            slide2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(infos[0].Color));
            slide2.submitButton.Click += SubmitButton_Click;
        }
        private static void ResetCheckboxes()
        {
            int i = 0;
            foreach (StackPanel stack in stacks)
            {
                currentScaleIndex = i;
                foreach (CheckBox checkBox in stack.Children)
                {
                    if (!(bool)checkBox.IsChecked)
                    {
                        checkBox.IsChecked = true;
                    }
                }
                i++;
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
            bool validate = true; string errors = "Số đại biểu được bầu khác số lượng yêu cầu tại:";
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].TotalVoted != infos[i].Max)
                {
                    validate = false;
                    errors += "\n - Phiếu bầu " + infos[i].Title;
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
                dialogs.ShowTextDialog(errors, "Trở lại");
            }
            else
            {
                dialogs.ShowLoadingDialog();
                for (int i = 0; i < infos.Count; i++)
                {
                    for (int j = 0; j < scales[i].Count; j++)
                    {
                        string name = scales[i][j].Name.Replace("\'", "\'\'");
                        // Save stuff to db file
                        string query = $"UPDATE \"{infos[i].Scale}\" SET Votes = {scales[i][j].Votes} WHERE Name = '{name}';";
                        _ = await connection.ExecuteAsync(query);
                    }
                }
                dialogs.CloseDialog();

                dialogs.ShowTextDialog("Đã nộp phiếu bầu. Chúc một ngày tốt lành.", "Đóng", () =>
                {
                    PreviousPage();
                    ResetCheckboxes();
                    WaitForSignal();
                });
            }
        }
        /// <summary>
        /// Occur when slide is changed with change slide radio buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeSlide_Checked(object sender, RoutedEventArgs e)
        {
            currentScaleIndex = slide2.votePanel.Children.IndexOf((UIElement)sender);
            slide2.voteStack.Children.Clear();
            _ = slide2.voteStack.Children.Add(stacks[currentScaleIndex]);

            slide2.title.Text = "Đại biểu " + infos[currentScaleIndex].Title + " " + infos[currentScaleIndex].Year;
            slide2.caption.Text = "Chọn đúng " + infos[currentScaleIndex].Max + " người";
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
