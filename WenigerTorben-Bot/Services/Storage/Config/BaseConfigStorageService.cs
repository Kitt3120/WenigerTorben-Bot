using System;
using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Storage.Config;

public abstract class BaseConfigStorageService<T> : AsyncStorageService<T>, IConfigStorageService<T>
{
    public override ServicePriority Priority => ServicePriority.Essential;

    public BaseConfigStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    protected override string GetTopLevelDirectoryName() => "Configs";

    public override string GetDirectory()
    {
        if (PlatformUtils.GetOSPlatform() == PlatformID.Unix)
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "WenigerTorben-Bot", GetStorageDirectoryName());
        return base.GetDirectory();
    }

    protected override string GetStorageFileExtension() => "json";

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