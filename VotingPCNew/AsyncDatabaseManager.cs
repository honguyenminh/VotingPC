using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using VotingPC;

namespace VotingPCNew;

public class AsyncDatabaseManager : IDisposable
{
    private bool _disposed;
    private readonly bool _multipleFile;
    private SQLiteAsyncConnection _inputConnection;
    private readonly List<SQLiteAsyncConnection> _connections = new();
    public List<Sector> SectorList { get; private set; }

    public AsyncDatabaseManager(bool saveToMultipleFile = false)
    {
        _multipleFile = saveToMultipleFile;
    }
    
    /// <summary>
    /// Try to open a connection to SQLite database
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
        foreach (Sector sector in SectorList)
        {
            string escaped = sector.Name.Replace("'", "''");
            sector.Candidates = await _inputConnection.QueryAsync<Candidate>($"SELECT * FROM '{escaped}'");
        }

        if (_multipleFile) await _inputConnection.CloseAsync();
    }

    public bool Validate()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().ToString());
        foreach (Sector sector in SectorList)
        {
            if (!sector.IsValid) return false;
            foreach (Candidate candidate in sector.Candidates)
            {
                if (!candidate.IsValid) return false;
                candidate.Votes = 0;
            }
        }
        return true;
    }

    public async Task SplitFiles(string folderPath, string password)
    {
        if (!_multipleFile) return;
        if (Extensions.FolderIsReadOnly(folderPath)) throw new IOException("Folder is readonly");
        for (int i = 0; i < SectorList.Count; i++)
        {
            string path = Path.Join(folderPath, SectorList[i].Name + ".db");
            if (File.Exists(path)) File.Delete(path);

            SQLiteConnectionString newOptions = new(path, true, password);
            SQLiteAsyncConnection newConnection = new(newOptions);
            
            await CreateCloneTable(newConnection, SectorList[i]);

            _connections.Add(newConnection);
        }
    }

    private static async Task CreateCloneTable(SQLiteAsyncConnection connection,
        Sector info)
    {
        // Create Info table then add info row to table
        await connection.CreateTableAsync<Sector>();
        await connection.InsertOrReplaceAsync(info);
        // Create Sector table then add candidates
        string escapedTableName = info.Name.Replace("'", "''");
        await connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS '{escapedTableName}' (" +
                                               "'Name' TEXT NOT NULL UNIQUE," +
                                               "'Votes' INTEGER NOT NULL DEFAULT 0," +
                                               "'Gender' TEXT NOT NULL," +
                                               "PRIMARY KEY('Name'))");
        await InsertAllCandidateAsync(connection, escapedTableName, info.Candidates);
    }

    /// <summary>
    /// Insert all candidates into the provided table name. Because SQLite-net is stupid and doesn't have this.
    /// </summary>
    /// <param name="connection">SQLite connection to insert into</param>
    /// <param name="escapedTableName">The table name to insert into with ' characters escaped</param>
    /// <param name="candidates">Candidates to insert into table</param>
    private static async Task InsertAllCandidateAsync(SQLiteAsyncConnection connection,
        string escapedTableName, IEnumerable<Candidate> candidates)
    {
        List<string> data = new();
        foreach (var candidate in candidates)
        {
            string escapedName = candidate.Name.Replace("'", "''");
            data.Add($"('{escapedName}', {candidate.Votes}, '{candidate.Gender}')");
        }
        string dataString = string.Join(',', data);
        string query = $"INSERT INTO '{escapedTableName}' (Name, Votes, Gender) VALUES {dataString}";
        await connection.ExecuteAsync(query);
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