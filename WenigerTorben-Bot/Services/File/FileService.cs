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
        string dataDirectory = GetDataDirectory();
        if (!Directory.Exists(dataDirectory))
        {
            Serilog.Log.Debug("Creating directory {directory}", dataDirectory);
            Directory.CreateDirectory(dataDirectory);
        }
    }

    public string GetDataDirectory()
    {
        return PlatformUtils.GetOSPlatform() switch
        {
            PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WenigerTorben-Bot"),
            PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "WenigerTorben-Bot"),
            _ => "."
        };
    }

    public string GetPath(params string[] paths) => Path.Combine(GetDataDirectory(), Path.Combine(paths));

    public string GetAndCreateDirectory(params string[] paths)
    {
        string path = GetPath(paths);
        if (!Directory.Exists(path))
        {
            Serilog.Log.Debug("Creating directory {directory}", path);
            Directory.CreateDirectory(path);
        }
        return path;
    }

    public string GetAppDomainPath() => AppDomain.CurrentDomain.BaseDirectory;

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}