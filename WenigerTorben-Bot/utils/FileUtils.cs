using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace WenigerTorbenBot.Utils;
public class FileUtils
{
    public static string DataPath { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}WenigerTorbenBot";
    public static string ConfigPath { get; } = $"{GetConfigDirectory()}{Path.DirectorySeparatorChar}config.json";

    public static string GetConfigDirectory()
    {
        return PlatformUtils.GetOSPlatform() switch
        {
            PlatformID.Win32NT => DataPath,
            PlatformID.Unix => $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}.config{Path.DirectorySeparatorChar}WenigerTorbenBot",
            _ => string.Empty,
        };
    }

    public static void GenerateDirectories()
    {
        Directory.CreateDirectory(DataPath);
        if (PlatformUtils.GetOSPlatform() == PlatformID.Unix)
            Directory.CreateDirectory(GetConfigDirectory());
    }

    public static string GetPath(params string[] paths) => $"{DataPath}{Path.DirectorySeparatorChar}{string.Join(Path.DirectorySeparatorChar, paths)}";

    public static string GetAndCreateDirectory(params string[] paths)
    {
        string path = GetPath(paths);
        Directory.CreateDirectory(path);
        return path;
    }
}