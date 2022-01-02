using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace VotingPCNew;

internal static class Extensions
{
    private static readonly ImageSourceConverter imageConverter = new();

    public static void SetConfig(this TextBlock textBlock, TextConfig config)
    {
        textBlock.Text = config.Text ?? "";
        if (config.Size != -1) textBlock.FontSize = config.Size;
        textBlock.FontStyle = config.Style;
        textBlock.FontWeight = config.Weight;
    }

    public static void SetSource(this Image image, string path)
    {
        image.Source = (ImageSource) imageConverter.ConvertFromString(path);
    }

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

    public static async Task<bool> FileIsReadOnly(string path)
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

    public static bool FolderIsReadOnly(string path)
    {
        DirectoryInfo directoryInfo = new(path);
        return directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
    }
}