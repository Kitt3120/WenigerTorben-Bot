using System;
using System.IO;

namespace WenigerTorbenBot.Utils;
public class FileUtils
{
    public static string DataPath { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}WenigerTorbenBot";
    public static void GenerateDirectories()
    {
        Directory.CreateDirectory(DataPath);
    }

    public static string GetPath(params string[] paths) => $"{DataPath}{Path.DirectorySeparatorChar}{string.Join(Path.DirectorySeparatorChar, paths)}";
}