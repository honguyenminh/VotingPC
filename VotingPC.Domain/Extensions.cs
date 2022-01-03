using System.Globalization;

namespace VotingPC.Domain;

public static class Extensions
{
    public static string ToTitleCase(this string str)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
    }
    // public static async Task<bool> TableExistsAsync(this SQLiteAsyncConnection connection, string name)
    // {
    //     string query = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}'";
    //     var result = await connection.QueryAsync<Master>(query);
    //     if (result.Count == 0) return false;
    //     return true;
    // }

    public static async Task<bool> FileIsReadOnly(this string path)
    {
        try
        {
            await using FileStream file = new(path, FileMode.Open, FileAccess.ReadWrite);
            return false;
        }
        catch
        {
            return true;
        }
    }

    public static bool FolderIsReadOnly(this string path)
    {
        DirectoryInfo directoryInfo = new(path);
        return directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
    }
}