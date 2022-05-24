using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Config;

public class ConfigService : Service, IConfigService
{
    private Dictionary<string, object> properties;

    public ConfigService() : base("Config")
    { }

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

    public void Set<T>(string key, T value) => properties[key] = value;

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

    public void Load()
    {
        if (File.Exists(FileUtils.ConfigPath))
            properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(FileUtils.ConfigPath));
        else
            properties = new Dictionary<string, object>();
    }

    public async Task LoadAsync()
    {
        if (File.Exists(FileUtils.ConfigPath))
            properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(await File.ReadAllTextAsync(FileUtils.ConfigPath));
        else
            properties = new Dictionary<string, object>();
    }

    public void Save() => File.WriteAllText(FileUtils.ConfigPath, JsonConvert.SerializeObject(properties));

    public async Task SaveAsync() => await File.WriteAllTextAsync(FileUtils.ConfigPath, JsonConvert.SerializeObject(properties));

    protected override ServiceConfiguration GetServiceConfiguration()
    {
        return new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();
    }
}