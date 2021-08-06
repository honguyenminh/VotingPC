using SQLite;
using Microsoft.Win32;
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
        private static readonly List<SQLiteAsyncConnection> connectionList = new();
        private static SerialPort serial;
        private static bool isListening = true;
        private static readonly List<List<Candidate>> sectionList = new();
        // List of information about sections
        private static List<Info> infoList;
        // Stack panel list which contain the vote UI, to preserve their state while user switch between sections
        private static readonly List<StackPanel> candidateLists = new();
        private static int currentSectionIndex;
        private static bool saveToMultipleFile;

        private bool ShowOpenDatabaseDialog()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Database file (*.db)|*.db",
                Multiselect = false,
                Title = "Chọn tệp cơ sở dữ liệu",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                databasePath = openFileDialog.FileName;
                return true;
            }
            else
            {
                Close();
                return false;
            }
        }
        private static async Task CreateCloneTable(SQLiteAsyncConnection cloneConnection, Info info, List<Candidate> candidateList)
        {
            // Create Info table then add info row to table
            await cloneConnection.ExecuteAsync("CREATE TABLE 'Info' (\n" +
                "'Section' TEXT NOT NULL UNIQUE,\n" +
                "'Max'   INTEGER NOT NULL,\n" +
                "'Color' TEXT NOT NULL DEFAULT '#111111',\n" +
                "'Title' TEXT NOT NULL,\n" +
                "'Year'  TEXT NOT NULL,\n" +
                "PRIMARY KEY('Section')\n)");
            await cloneConnection.InsertOrReplaceAsync(info);
            // Create section table then add candidates
            await cloneConnection.ExecuteAsync($"CREATE TABLE 'Candidate' (\n" +
                "'Name' TEXT NOT NULL UNIQUE,\n" +
                "'Votes'   INTEGER NOT NULL DEFAULT 0,\n" +
                "'Gender' TEXT NOT NULL,\n" +
                "PRIMARY KEY('Name')\n)");
            await cloneConnection.InsertAllAsync(candidateList);
            await cloneConnection.ExecuteAsync($"ALTER TABLE Candidate RENAME TO {info.Sector};");
        }
        private void InvalidDatabase()
        {
            dialogs.CloseDialog();
            dialogs.ShowTextDialog("Cơ sở dữ liệu không hợp lệ, vui lòng kiểm tra lại.", "Đóng", Close);
        }
        /// <summary>
        /// Validate the database, then reset the votes
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
                    Content = info.Sector,
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
    }
}
