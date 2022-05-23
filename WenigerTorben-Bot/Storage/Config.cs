using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Storage;
public class Config
{
    public string Path { get; } = FileUtils.GetPath("WenigerTorbenBot.conf");
    private readonly Dictionary<string, object> properties;

    public Config()
    {
        if (File.Exists(Path))
            properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path));
        else
            properties = new Dictionary<string, object>();
    }

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

    public async Task SaveAsync() => await File.WriteAllTextAsync(Path, JsonConvert.SerializeObject(properties));

}