using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
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
        private static readonly List<List<Candidate>> sectionList = new();
        private static List<Info> infoList; // List of information about sections
        // Contains candidate list stack panels, to preserve their state while user switch between sections
        private static readonly List<StackPanel> candidateLists = new();
        private static int currentSectionIndex;

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
                    "Mã lỗi: " + e.Message, "OK", Close);
                return;
            }
            // When no serial port is found, dump error message box, then quit
            if (serial == null)
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("Không tìm thấy thiết bị Arduino. Vui lòng gọi kỹ thuật viên.", "OK", Close);
                return;
            }

            try { await LoadDatabase(); }
            catch (Exception) { InvalidDatabase(); return; }

            if (ValidateDatabase() == false)
            {
                InvalidDatabase();
                return;
            }

            // TODO: Show database warnings/errors here

            PopulateVoteUI();
            dialogs.CloseDialog();  // Close loading dialog opened above
            WaitForSignal();        // Wait for serial signal from Arduino
        }
        private void InvalidDatabase()
        {
            dialogs.CloseDialog();
            dialogs.ShowTextDialog("Cơ sở dữ liệu không hợp lệ, vui lòng kiểm tra lại.", "Đóng", Close);
        }
        /// <summary>
        /// Load database into infos and sections Lists
        /// </summary>
        /// <returns>An awaitable Task that do the work</returns>
        private static async Task LoadDatabase()
        {
            string query = $"SELECT * FROM Info";
            infoList = await connection.QueryAsync<Info>(query);
            query = $"SELECT * FROM \"";
            foreach (Info info in infoList)
            {
                sectionList.Add(await connection.QueryAsync<Candidate>(query + info.Section + "\""));
            }
        }
        /// <summary>
        /// Validate the database. Will reset the votes too
        /// </summary>
        /// <returns>True if is valid, else false</returns>
        private static bool ValidateDatabase()
        {
            foreach (Info info in infoList)
            {
                if (!info.IsValid) return false;
            }
            foreach (List<Candidate> candidateList in sectionList)
            {
                foreach (Candidate candidate in candidateList)
                {
                    if (!candidate.IsValid) return false;
                    candidate.Votes = 0;
                }
            }
            return true;
        }
        /// <summary>
        /// Use loaded database lists to populate vote UI (Section radio buttons, candidates, (sub)title)
        /// </summary>
        private void PopulateVoteUI()
        {
            // This hard-coded ui is utterly stupid.
            // TODO: make a user control for the VoteUI
            int index = 0;
            foreach (Info info in infoList)
            {
                // Add a button to section chooser card on top left
                RadioButton sectionButton = new()
                {
                    Style = (Style)Application.Current.Resources["MaterialDesignTabRadioButton"],
                    Margin = new Thickness(4),
                    Content = info.Section,
                    IsChecked = false
                };
                sectionButton.Checked += ChangeSlide_Checked;
                _ = slide2.votePanel.Children.Add(sectionButton);


                // Add new page as StackPanel to the voteStack on right hand
                StackPanel stackPanel = new()
                {
                    Margin = new Thickness(72, 40, 72, 40),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                currentSectionIndex = index;
                foreach (Candidate item in sectionList[index])
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
                        if (((CheckBox)sender).Tag != null) return;

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
                        // Mark that this checkbox already has its length checked
                        checkBox.Tag = "Tagged";
                    };

                    _ = stackPanel.Children.Add(checkBox);
                }
                candidateLists.Add(stackPanel);
                index++;
            }
            slide2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(infoList[0].Color));
            slide2.submitButton.Click += SubmitButton_Click;
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
        private static void ResetCheckboxes()
        {
            int i = 0;
            foreach (StackPanel stack in candidateLists)
            {
                currentSectionIndex = i;
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
        /// <summary>
        /// Get theoretical size on screen of a text block
        /// </summary>
        /// <param name="textBlock"></param>
        /// <returns>A Size object</returns>
        private static Size GetTheoreticalSize(TextBlock textBlock)
        {
            // This is hard to read, yes. So, like timezone code, ignore it and just believe that it will work
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
            infoList[currentSectionIndex].TotalVoted++;
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            infoList[currentSectionIndex].TotalVoted--;
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            bool validate = true; string errors = "Số đại biểu được bầu khác số lượng yêu cầu tại:";
            for (int i = 0; i < infoList.Count; i++)
            {
                if (infoList[i].TotalVoted != infoList[i].Max)
                {
                    validate = false;
                    errors += "\n - Phiếu bầu " + infoList[i].Title;
                    continue;
                }
                for (int j = 0; j < sectionList[i].Count; j++)
                {
                    if ((bool)((CheckBox)candidateLists[i].Children[j]).IsChecked)
                    {
                        sectionList[i][j].Votes++;
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
                for (int i = 0; i < infoList.Count; i++)
                {
                    for (int j = 0; j < sectionList[i].Count; j++)
                    {
                        string name = sectionList[i][j].Name.Replace("\'", "\'\'");
                        // Save stuff to db file
                        string query = $"UPDATE \"{infoList[i].Section}\" SET Votes = {sectionList[i][j].Votes} WHERE Name = '{name}';";
                        _ = await connection.ExecuteAsync(query);
                    }
                }
                dialogs.CloseDialog();

                dialogs.ShowTextDialog("Đã nộp phiếu bầu. Chúc một ngày tốt lành.", "Đóng", async () =>
                {
                    PreviousPage();
                    ResetCheckboxes();
                    await Task.Delay(1000);
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
            currentSectionIndex = slide2.votePanel.Children.IndexOf((UIElement)sender);
            slide2.voteStack.Children.Clear();
            _ = slide2.voteStack.Children.Add(candidateLists[currentSectionIndex]);

            slide2.title.Text = "Đại biểu " + infoList[currentSectionIndex].Title + " " + infoList[currentSectionIndex].Year;
            slide2.caption.Text = "Chọn đúng " + infoList[currentSectionIndex].Max + " người";
            slide2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(infoList[currentSectionIndex].Color));
            slide2.voteCard.MinWidth = slide2.mainGrid.ColumnDefinitions[1].ActualWidth - 120;
        }
        #endregion
    }
}
