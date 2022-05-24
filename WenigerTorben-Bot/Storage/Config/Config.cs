using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Storage.Config;
public class Config : IConfig
{
    public string Path { get; } = FileUtils.GetPath("WenigerTorbenBot.conf");
    private Dictionary<string, object> properties;

    public Config(bool autoLoad = false)
    { }

    public bool Exists(string key) => properties.ContainsKey(key);
    public object Get(string key) => properties[key];
    public T Get<T>(string key) => (T)properties[key];
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
    public object this[string key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    public void Load()
    {
        if (File.Exists(Path))
            properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path));
        else
            properties = new Dictionary<string, object>();
    }

    public async Task LoadAsync()
    {
        if (File.Exists(Path))
            properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(await File.ReadAllTextAsync(Path));
        else
            properties = new Dictionary<string, object>();
    }

    public void Save() => File.WriteAllText(Path, JsonConvert.SerializeObject(properties));

    public async Task SaveAsync() => await File.WriteAllTextAsync(Path, JsonConvert.SerializeObject(properties));

}