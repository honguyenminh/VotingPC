using SQLite;

namespace VotingPC.Domain.Extensions;

public static class SqliteConnectionExtensions
{
    public static async Task CreateCloneTableAsync(this SQLiteAsyncConnection connection,
        Sector sector)
    {
        // Create master table then add info row to table
        await connection.CreateTableAsync<Sector>();
        await connection.InsertOrReplaceAsync(sector);
        // Create Sector table then add candidates
        await connection.CreateCandidateTableAsync(sector.Name);
        await connection.InsertAllCandidateAsync(sector.Name, sector.Candidates);
    }

    /// <summary>
    ///     Insert all candidates into the provided table name, create the table if not exists.<br/>
    ///     Because SQLite-net is stupid and doesn't have this.
    /// </summary>
    /// <param name="connection">SQLite connection to insert into</param>
    /// <param name="tableName">The table name to insert into</param>
    /// <param name="candidates">Candidates to insert into table</param>
    /// <returns>Number of rows modified by this operation</returns>
    public static async Task<int> InsertAllCandidateAsync(this SQLiteAsyncConnection connection,
        string tableName, IEnumerable<Candidate> candidates)
    {
        if (tableName.StartsWith("sqlite_"))
            throw new ArgumentException("Reserved table name (sqlite_)", nameof(tableName));
        string escapedTableName = tableName.Replace("'", "''");
        List<string> data = new();
        foreach (var candidate in candidates)
        {
            string escapedName = candidate.Name.Replace("'", "''");
            string escapedGender = candidate.Gender.Replace("'", "''");
            data.Add($"('{escapedName}', {candidate.Votes}, '{escapedGender}')");
        }

        string dataString = string.Join(',', data);
        string query = $"INSERT INTO '{escapedTableName}' (Name, Votes, Gender) VALUES {dataString}";
        return await connection.ExecuteAsync(query);
    }

    /// <summary>
    ///     Create a new table with Candidate schema and the given table name
    /// </summary>
    /// <param name="connection">The connection to create on</param>
    /// <param name="tableName">Table's unescaped name</param>
    /// <returns></returns>
    public static async Task<int> CreateCandidateTableAsync(this SQLiteAsyncConnection connection,
        string tableName)
    {
        if (tableName.StartsWith("sqlite_"))
            throw new ArgumentException("Reserved table name (sqlite_)", nameof(tableName));
        string escapedTableName = tableName.Replace("'", "''");
        return await connection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS '{escapedTableName}' (" +
                                      "'Name' TEXT NOT NULL UNIQUE," +
                                      "'Votes' INTEGER NOT NULL DEFAULT 0," +
                                      "'Gender' TEXT NOT NULL," +
                                      "PRIMARY KEY('Name'))");
    }
}