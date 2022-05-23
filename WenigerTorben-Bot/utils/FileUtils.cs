using System;
using System.IO;

namespace WenigerTorbenBot.Utils;
public class FileUtils
{
    public static string DataPath { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.PathSeparator}WenigerTorbenBot";
    public static void GenerateDirectories()
    {
        Directory.CreateDirectory(DataPath);
    }

    public static string GetPath(params string[] paths) => $"{DataPath}{Path.PathSeparator}{string.Join(Path.PathSeparator, paths)}";
}