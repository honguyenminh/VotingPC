using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VotingPC
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Click handler for the Password Dialog button.
        /// </summary>
        private async void PasswordDialogButton_Click(object sender, RoutedEventArgs e)
        {
            dialogs.CloseDialog();
            dialogs.ShowLoadingDialog();

            // Create new SQLite database connection
            SQLiteConnectionString options = new(databasePath, storeDateTimeAsTicks: true, passwordDialog.Password);
            SQLiteAsyncConnection connection = new(options);

            try
            {
                //This will try to query the SQLite Schema Database, if the key is correct then no exception is raised
                _ = await connection.QueryAsync<int>("SELECT count(*) FROM sqlite_master");
            }
            catch (SQLiteException) // Wrong password
            {
                dialogs.CloseDialog();
                await connection.CloseAsync();
                await Task.Delay(3000); // Delay 3 seconds if wrong password, reduce chance of "brute-force"
                // Request password from user again
                passwordDialog.Show(true);
                return;
            }

            // If thing goes well aka correct password, init the app
            // Get serial port
            try
            {
                serial = await Task.Run(ArduinoInteract.GetArduinoCOMPort);
            }
            catch (Exception err)
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("Lỗi tìm thiết bị Arduino. Vui lòng gọi kỹ thuật viên.\n" +
                    "Mã lỗi: " + err.Message, "OK", Close);
                return;
            }
            // When no serial port is found, dump error message box, then quit
            if (serial == null)
            {
                dialogs.CloseDialog();
                dialogs.ShowTextDialog("Không tìm thấy thiết bị Arduino. Vui lòng gọi kỹ thuật viên.", "OK", Close);
                return;
            }

            // Load input database file to Lists
            try
            {
                string query = $"SELECT * FROM Info";
                infoList = await connection.QueryAsync<Info>(query);
                query = $"SELECT * FROM \"";
                foreach (Info info in infoList)
                {
                    sectionList.Add(await connection.QueryAsync<Candidate>(query + info.Sector + "\""));
                }
            }
            catch (Exception) { InvalidDatabase(); return; }

            if (ValidateDatabase() == false)
            {
                InvalidDatabase();
                return;
            }

            // Separate the sector if save to multiple file
            if (saveToMultipleFile)
            {
                // Close current "input file" connection first
                await connection?.CloseAsync();
                for (int i = 0; i < infoList.Count; i++)
                {
                    // TODO: Check if we have write priviledge
                    string path = $"{folderPath}\\{infoList[i].Sector}.db";
                    try { if (File.Exists(path)) File.Delete(path); }
                    catch
                    {
                        dialogs.ShowTextDialog("File cơ sở dữ liệu đã tồn tại đang được sử dụng bởi\n" +
                            "ứng dụng khác, không thế xóa.\n" +
                            "Tại: " + path, "OK", Close);
                        return;
                    }

                    SQLiteConnectionString newOptions = new(path, storeDateTimeAsTicks: true, passwordDialog.Password);
                    SQLiteAsyncConnection newConnection = new(newOptions);
                    try
                    {
                        await CreateCloneTable(newConnection, infoList[i], sectionList[i]);
                    }
                    catch
                    {
                        dialogs.CloseDialog();
                        dialogs.ShowTextDialog("Lỗi tạo file cơ sở dữ liệu.\n" +
                            "Kiểm tra xem tên sector có hợp lệ để tạo tên tệp không và thử lại.", "OK", Close);
                        return;
                    }

                    connectionList.Add(newConnection);
                }
            }
            else
            {
                connectionList.Add(connection);
            }

            // TODO: Show database warnings/errors here
            // Await for PopulateVoteUI. This is stupid.
            await Application.Current.Dispatcher.InvokeAsync(PopulateVoteUI);
            dialogs.CloseDialog();
            WaitForSignal(); // Wait for serial signal from Arduino
        }

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
            bool isValid = true; string errors = "Số đại biểu được bầu khác số lượng yêu cầu tại:";
            for (int i = 0; i < infoList.Count; i++)
            {
                if (infoList[i].TotalVoted != infoList[i].Max)
                {
                    isValid = false;
                    errors += "\n - Phiếu bầu " + infoList[i].Title;
                    continue;
                }
                if (isValid == false) continue;
                for (int j = 0; j < sectionList[i].Count; j++)
                {
                    if ((bool)((CheckBox)candidateLists[i].Children[j]).IsChecked)
                    {
                        sectionList[i][j].Votes++;
                    }
                }
            }

            if (!isValid)
            {
                dialogs.ShowTextDialog(errors, "Trở lại");
            }
            else
            {
                dialogs.ShowLoadingDialog();
                // For each sector
                SQLiteAsyncConnection connection = connectionList[0];
                for (int i = 0; i < infoList.Count; i++)
                {
                    if (saveToMultipleFile) connection = connectionList[i];
                    string escapedSector = infoList[i].Sector.Replace("'", "''");
                    // For each candidate in sector
                    for (int j = 0; j < sectionList[i].Count; j++)
                    {
                        // Escape the quotes in strings
                        string name = sectionList[i][j].Name.Replace("'", "''");
                        // Add details to query command
                        string query = $"UPDATE '{escapedSector}' SET Votes = {sectionList[i][j].Votes} WHERE Name = '{name}';";
                        await connection.ExecuteAsync(query);
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
        /// Occur when slide is changed with change slide radio buttons on top left
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
    }
}
