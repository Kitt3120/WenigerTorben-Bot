using System;
using System.Data.Common;
using System.IO;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.File;

public class FileService : Service, IFileService
{
    public override string Name => "File";

    public override ServicePriority Priority => ServicePriority.Essential;

    protected override void Initialize()
    {
        Directory.CreateDirectory(GetDataDirectory());
    }

    public string GetDataDirectory()
    {
        return PlatformUtils.GetOSPlatform() switch
        {
            PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WenigerTorben-Bot"),
            PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "WenigerTorben-Bot"),
            _ => string.Empty
        };
    }

    public string GetPath(params string[] paths) => Path.Combine(GetDataDirectory(), Path.Combine(paths));

    public string GetAndCreateDirectory(params string[] paths)
    {
        string path = GetPath(paths);
        Directory.CreateDirectory(path);
        return path;
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}