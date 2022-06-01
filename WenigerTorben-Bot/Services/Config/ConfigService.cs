using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Config;

public class ConfigService : Service, IConfigService, IAsyncDisposable
{
    public override string Name => "Config";
    public override ServicePriority Priority => ServicePriority.Essential;

    private readonly IFileService fileService;

    private Dictionary<string, IConfig> configs = new Dictionary<string, IConfig>();

    public ConfigService(IFileService fileService) : base()
    {
        this.fileService = fileService;
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

    public bool Exists(string guildId = "global") => configs.ContainsKey(guildId);

    public IConfig Get(string guildId = "global") => configs[guildId];

    public void Delete(string guildId)
    {
        configs[guildId].Delete();
        configs.Remove(guildId);
    }

    public void Load(string guildId = "global")
    {
        if (Exists(guildId))
        {
            Serilog.Log.Error("Error while loading config {guildId}: Config already loaded.", guildId);
            return;
        }

        IConfig config = new Storage.Config.Config(GetConfigFilePath(guildId));
        config.Load();
        configs[guildId] = config;
    }

    public async Task LoadAsync(string guildId = "global")
    {

        if (Exists(guildId))
        {
            Serilog.Log.Error("Error while loading config {guildId}: Config already loaded.", guildId);
            return;
        }

        IConfig config = new Storage.Config.Config(GetConfigFilePath(guildId));
        await config.LoadAsync();
        configs[guildId] = config;
    }

    public void LoadAll()
    {
        foreach (string configPath in Directory.GetFiles(GetConfigsDirectory()))
        {
            string guildId = Path.GetFileNameWithoutExtension(configPath);
            IConfig config = new Storage.Config.Config(GetConfigFilePath(guildId));
            config.Load();
            configs[guildId] = config;
        }
    }

    public async Task LoadAllAsync()
    {
        foreach (string configPath in Directory.GetFiles(GetConfigsDirectory()))
        {
            string guildId = Path.GetFileNameWithoutExtension(configPath);
            IConfig config = new Storage.Config.Config(GetConfigFilePath(guildId));
            await config.LoadAsync();
            configs[guildId] = config;
        }
    }

    public void Save(string guildId = "global") => configs[guildId].Save();

    public async Task SaveAsync(string guildId = "global") => await configs[guildId].SaveAsync();

    public void SaveAll()
    {
        foreach (IConfig config in configs.Values)
            config.Save();
    }

    public async Task SaveAllAsync() => await Task.WhenAll(configs.Values.Select(config => config.SaveAsync()).ToArray());

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();

    public async ValueTask DisposeAsync() => await SaveAllAsync();
}