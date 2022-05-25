using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Config;

public class ConfigService : Service, IConfigService, IAsyncDisposable
{
    public override string Name => "Config";
    public override ServicePriority Priority => ServicePriority.Essential;

    private readonly IFileService fileService;
    private Dictionary<string, object> properties = new Dictionary<string, object>();


    public ConfigService(IFileService fileService) : base()
    {
        this.fileService = fileService;
    }

    protected override async Task InitializeAsync() => await LoadAsync();

    public bool Exists(string key) => properties.ContainsKey(key);

    public object Get(string key) => properties[key];

    public T Get<T>(string key) => (T)properties[key];

    public object this[string key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    public void Set(string key, object value) => properties[key] = value;

    public object GetOrSet(string key, object defaultValue)
    {
        if (Exists(key))
            return Get(key);
        else
        {
            Set(key, defaultValue);
            return defaultValue;
        }
    }

    public T GetOrSet<T>(string key, T defaultValue)
    {
        if (Exists(key))
            return Get<T>(key);
        else
        {
            Set(key, defaultValue);
            return defaultValue;
        }
    }

    public void Remove(string key) => properties.Remove(key);

    //TODO: Null-Check
    public void Load()
    {
        if (System.IO.File.Exists(fileService.ConfigPath))
        {
            Dictionary<string, object>? deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(fileService.ConfigPath));
            if (deserialized is not null)
                properties = deserialized;
        }
    }

    public async Task LoadAsync()
    {
        
        if (System.IO.File.Exists(fileService.ConfigPath))
        {
            Dictionary<string, object>? deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(await System.IO.File.ReadAllTextAsync(fileService.ConfigPath));
            if (deserialized is not null)
                properties = deserialized;
        }
    }

    public void Save()
    {
        if(Status == ServiceStatus.Started)
            System.IO.File.WriteAllText(fileService.ConfigPath, JsonConvert.SerializeObject(properties));
    }
    public async Task SaveAsync()
    {
        if (Status == ServiceStatus.Started)
            await System.IO.File.WriteAllTextAsync(fileService.ConfigPath, JsonConvert.SerializeObject(properties));
    }
    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();

    public async ValueTask DisposeAsync() => await SaveAsync();
}