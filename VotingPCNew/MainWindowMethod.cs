using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VotingPCNew;

// Contains extra Init/Parse methods for main window
public partial class MainWindow
{
    private static async Task<bool> FileIsReadOnly(string path)
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

    private static bool FolderIsReadOnly(string path)
    {
        DirectoryInfo directoryInfo = new(path);
        return directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
    }
}
