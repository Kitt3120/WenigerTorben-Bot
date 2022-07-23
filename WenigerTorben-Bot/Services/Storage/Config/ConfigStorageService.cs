using System;
using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Storage.Config;

public class ConfigStorageService<T> : AsyncStorageService<T>, IConfigStorageService<T>
{
    public override string Name => "ConfigStorage";
    public override ServicePriority Priority => ServicePriority.Essential;

    public ConfigStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string GetDirectory()
    {
        if (PlatformUtils.GetOSPlatform() == PlatformID.Unix && customDirectory is null)
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "WenigerTorben-Bot");
        else
            return Path.Combine(fileService.GetDataDirectory(), GetDefaultDirectory());
    }

    public override string GetDefaultDirectory() => "Configs";

    public override string GetFileExtension() => "json";

    public override void Load(string identifier = "global")
    {
        IAsyncStorage<T> storage = new ConfigStorage<T>(GetStorageFilePath(identifier));
        storage.Load();
        storages[identifier] = storage;
    }

    public override async Task LoadAsync(string identifier = "global")
    {
        IAsyncStorage<T> storage = new ConfigStorage<T>(GetStorageFilePath(identifier));
        await storage.LoadAsync();
        storages[identifier] = storage;
    }
}