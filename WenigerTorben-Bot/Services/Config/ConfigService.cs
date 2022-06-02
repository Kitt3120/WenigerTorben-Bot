using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Config;

public class ConfigService : Service, IConfigService, IAsyncDisposable
{
    public override string Name => "Config";
    public override ServicePriority Priority => ServicePriority.Essential;

    private readonly IFileService fileService;

    private Dictionary<string, IAsyncStorage<object>> configs;

    public ConfigService(IFileService fileService) : base()
    {
        this.fileService = fileService;
        this.configs = new Dictionary<string, IAsyncStorage<object>>();
    }

    protected override async Task InitializeAsync()
    {
        Directory.CreateDirectory(GetConfigsDirectory());
        await LoadAllAsync();
        if (!Exists())
            Load();
    }

    public string GetConfigsDirectory()
    {
        return PlatformUtils.GetOSPlatform() switch
        {
            PlatformID.Win32NT => Path.Combine(fileService.GetDataDirectory(), "Configs"),
            PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "WenigerTorben-Bot"),
            _ => Path.Combine(fileService.GetDataDirectory(), "Configs")
        };
    }

    public string GetConfigFilePath(string guildId = "global") => Path.Join(GetConfigsDirectory(), $"{guildId}.json");

    public IEnumerable<string> GetGuildIds() => configs.Keys.Where(key => key != "global");

    public bool Exists(string guildId = "global") => configs.ContainsKey(guildId) && configs[guildId] is not null;

    public IAsyncStorage<object>? Get(string guildId = "global")
    {
        if (Exists(guildId))
            return configs[guildId];
        else return null;
    }
    public void Delete(string guildId)
    {
        configs[guildId].Delete();
        configs.Remove(guildId);
    }

    public void Load(string guildId = "global")
    {
        IAsyncStorage<object> config = new ConfigStorage<object>(GetConfigFilePath(guildId));
        config.Load();
        configs[guildId] = config;
    }

    public async Task LoadAsync(string guildId = "global")
    {
        IAsyncStorage<object> config = new ConfigStorage<object>(GetConfigFilePath(guildId));
        await config.LoadAsync();
        configs[guildId] = config;
    }

    public void LoadAll()
    {
        foreach (string guildId in Directory.GetFiles(GetConfigsDirectory()).Select(configPath => Path.GetFileNameWithoutExtension(configPath)))
            Load(guildId);
    }

    public async Task LoadAllAsync() => await Task.WhenAll(Directory.GetFiles(GetConfigsDirectory()).Select(configPath => Path.GetFileNameWithoutExtension(configPath)).Select(guildId => LoadAsync(guildId)));

    public void Save(string guildId = "global") => Get(guildId)?.Save();

    public async Task SaveAsync(string guildId = "global")
    {
        // => await Get(guildId)?.SaveAsync(); gives warning here
        IAsyncStorage<object>? config = Get(guildId);
        if (config is not null)
            await config.SaveAsync();

        //TODO: Log when trying to save config that does not exist
    }
    public void SaveAll()
    {
        foreach (IAsyncStorage<object> config in configs.Values)
            config.Save();
    }

    public async Task SaveAllAsync() => await Task.WhenAll(configs.Values.Select(config => config.SaveAsync()).ToArray());

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();

    public async ValueTask DisposeAsync() => await SaveAllAsync();
}