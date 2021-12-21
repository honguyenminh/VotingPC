using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using VotingPC;

namespace VotingPCNew;

public class AsyncDatabaseManager : IDisposable
{
    private bool _disposed = false;
    private bool _multipleFile;
    private SQLiteAsyncConnection _inputConnection;
    private readonly List<SQLiteAsyncConnection> _connections = new();
    public List<Info> InfoList { get; private set; }
    public List<List<Candidate>> SectionList { get; } = new();

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
            return false;
        }
        _inputConnection = connection;
        return true;
    }

    public async Task Load()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().ToString());
        InfoList = await _inputConnection.QueryAsync<Info>("SELECT * FROM Info");
        SectionList.Clear();
        foreach (Info info in InfoList)
        {
            string escaped = info.Sector.Replace("\"", "\"\"");
            SectionList.Add(await _inputConnection.QueryAsync<Candidate>($"SELECT * FROM \"{escaped}\""));
        }
    }

    public bool Validate()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().ToString());
        foreach (Info info in InfoList)
        {
            if (!info.IsValid) return false;
        }
        foreach (List<Candidate> candidateList in SectionList)
        {
            foreach (Candidate candidate in candidateList)
            {
                if (!candidate.IsValid) return false;
                candidate.Votes = 0;
            }
        }
        return true;
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