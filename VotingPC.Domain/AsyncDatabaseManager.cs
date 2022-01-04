using SQLite;
using VotingPC.Domain.Extensions;

namespace VotingPC.Domain;

public class AsyncDatabaseManager : IDisposable
{
    private SQLiteAsyncConnection _inputConnection;
    private readonly List<SQLiteAsyncConnection> _connections = new();
    private readonly bool _multipleFile;
    private bool _disposed;

    public AsyncDatabaseManager(bool saveToMultipleFile = false)
    {
        _multipleFile = saveToMultipleFile;
    }

    public List<Sector> SectorList { get; private set; }

    /// <summary>
    ///     Try to open a connection to SQLite database
    /// </summary>
    /// <param name="path">Path to SQLite database file</param>
    /// <param name="password">Password used for decryption, use empty string if none was set</param>
    /// <returns>false if wrong password, true otherwise</returns>
    /// <exception cref="InvalidOperationException">Thrown if a connection is already opened</exception>
    public async Task<bool> Open(string path, string password = "")
    {
        if (_disposed) throw new ObjectDisposedException(GetType().ToString());
        if (_inputConnection is not null)
            throw new InvalidOperationException("A connection is already opened");
        SQLiteConnectionString options = new(path, true, password);
        SQLiteAsyncConnection connection = new(options);

        try
        {
            _ = await connection.QueryAsync<int>("SELECT count(*) FROM sqlite_master");
        }
        // Wrong password
        catch (SQLiteException)
        {
            await connection.CloseAsync();
            _inputConnection = null;
            return false;
        }

        _inputConnection = connection;
        return true;
    }

    public async Task Load()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().ToString());
        SectorList = await _inputConnection.Table<Sector>().ToListAsync();
        foreach (var sector in SectorList)
        {
            string escaped = sector.Name.Replace("'", "''");
            sector.Candidates = await _inputConnection.QueryAsync<Candidate>($"SELECT * FROM '{escaped}'");
        }

        if (_multipleFile) await _inputConnection.CloseAsync();
    }

    public bool Validate()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().ToString());
        foreach (var sector in SectorList)
        {
            if (!sector.IsValid) return false;
            foreach (var candidate in sector.Candidates)
            {
                if (!candidate.IsValid) return false;
                candidate.Votes = 0;
            }
        }

        return true;
    }

    public async Task SaveCurrentData()
    {
        var connection = _inputConnection;
        for (int i = 0; i < SectorList.Count; i++)
        {
            if (_multipleFile) connection = _connections[i];
            string escapedSectorName = SectorList[i].Name.Replace("'", "''");
            // For each candidate in sector
            foreach (var candidate in SectorList[i].Candidates)
            {
                // Escape the quotes in strings
                string name = candidate.Name.Replace("'", "''");
                // Add details to query command
                string query = $"UPDATE '{escapedSectorName}' SET Votes = {candidate.Votes} WHERE Name = '{name}'";
                await connection.ExecuteAsync(query);
            }
        }
    }

    public async Task SplitFiles(string folderPath, string password)
    {
        if (!_multipleFile) return;
        if (folderPath.FolderIsReadOnly()) throw new IOException("Folder is readonly");
        foreach (var sector in SectorList)
        {
            string path = Path.Join(folderPath, sector.Name + ".db");
            if (File.Exists(path)) File.Delete(path);

            SQLiteConnectionString newOptions = new(path, true, password);
            SQLiteAsyncConnection newConnection = new(newOptions);

            await newConnection.CreateCloneTableAsync(sector);

            _connections.Add(newConnection);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed) return;

        _inputConnection?.CloseAsync().GetAwaiter().GetResult();
        _inputConnection = null;
        if (_connections.Count != 0)
        {
            foreach (var connection in _connections)
            {
                connection.CloseAsync().GetAwaiter().GetResult();
            }

            _connections.Clear();
        }

        _disposed = true;
    }
}
