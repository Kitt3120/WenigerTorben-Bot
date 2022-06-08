using System;
using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Storage.Config;

public class ConfigService : AsyncStorageService, IConfigService
{
    public override string Name => "Config";
    public override ServicePriority Priority => ServicePriority.Essential;

    public ConfigService(IFileService fileService) : base(fileService)
    { }

    public override string GetDirectory()
    {
        return PlatformUtils.GetOSPlatform() switch
        {
            PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "WenigerTorben-Bot"),
            _ => Path.Combine(fileService.GetDataDirectory(), "Configs")
        };
    }

    public override string GetStorageFilePath(string identifier = "global") => Path.Join(GetDirectory(), $"{identifier}.json");

    public override void Load(string identifier = "global")
    {
        IAsyncStorage<object> config = new ConfigStorage<object>(GetStorageFilePath(identifier));
        config.Load();
        storages[identifier] = config;
    }

    public override async Task LoadAsync(string identifier = "global")
    {
        IAsyncStorage<object> config = new ConfigStorage<object>(GetStorageFilePath(identifier));
        await config.LoadAsync();
        storages[identifier] = config;
    }
}