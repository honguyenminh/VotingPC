using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VotingPCNew
{
    // Contains extra Init/Parse methods for main window
    public partial class MainWindow
    {
        // TODO: move to external DB class?
        /// <summary>
        /// Open a new SQLite database connection.
        /// </summary>
        /// <param name="databasePath">Path to SQLite database file</param>
        /// <param name="password">Password of database, leave empty string if none was set</param>
        /// <returns>The <see cref="SQLiteAsyncConnection"/> connection if opened successfully, <see langword="null"/> otherwise</returns>
        private static async Task<SQLiteAsyncConnection> OpenDatabaseAsync(string databasePath, string password)
        {
            SQLiteConnectionString options = new(databasePath, true, password);
            SQLiteAsyncConnection connection = new(options);

            try
            {
                _ = await connection.QueryAsync<int>("SELECT count(*) FROM sqlite_master");
            }
            // Wrong password
            catch (SQLiteException)
            {
                await connection.CloseAsync();
                return null;
            }
            return connection;
        }
    }
}
