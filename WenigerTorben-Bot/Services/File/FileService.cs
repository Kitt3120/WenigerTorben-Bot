using System;
using System.IO;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.File;

public class FileService : Service, IFileService
{
    public override string Name => "File";

    public override ServicePriority Priority => ServicePriority.Essential;

    public string DataPath => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}WenigerTorbenBot";

    public string ConfigPath => $"{GetConfigDirectory()}{Path.DirectorySeparatorChar}config.json";

    protected override void Initialize()
    {
        Directory.CreateDirectory(DataPath);
        if (PlatformUtils.GetOSPlatform() == PlatformID.Unix)
            Directory.CreateDirectory(GetConfigDirectory());
    }

    public string GetConfigDirectory()
    {
        return PlatformUtils.GetOSPlatform() switch
        {
            PlatformID.Win32NT => DataPath,
            PlatformID.Unix => $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}.config{Path.DirectorySeparatorChar}WenigerTorbenBot",
            _ => string.Empty,
        };
    }

    public string GetPath(params string[] paths) => $"{DataPath}{Path.DirectorySeparatorChar}{string.Join(Path.DirectorySeparatorChar, paths)}";

    public string GetAndCreateDirectory(params string[] paths)
    {
        string path = GetPath(paths);
        Directory.CreateDirectory(path);
        return path;
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}