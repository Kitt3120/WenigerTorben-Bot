using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace WenigerTorbenBot.Storage.Config;

public class Config : IConfig
{
    private readonly string filepath;

    private Dictionary<string, object> properties;


    public Config(string filepath)
    {
        this.filepath = filepath;

        this.properties = new Dictionary<string, object>();
    }

    public bool Exists(string key) => properties.ContainsKey(key) && properties[key] is not null;

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
            if (defaultValue is not null)
                Set(key, defaultValue);
            return defaultValue;
        }
    }

    public void Remove(string key) => properties.Remove(key);

    public void Load()
    {
        if (!File.Exists(filepath))
        {
            Log.Debug("No config found at {filepath}, using empty config", filepath);
            return;
        }

        try
        {
            Dictionary<string, object>? deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(filepath));
            if (deserialized is null)
                throw new Exception("Deserialized config was null"); //TODO: Proper exception
            properties = deserialized;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error loading config");
        }
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(filepath))
        {
            Log.Debug("No config found at {filepath}, using empty config", filepath);
            return;
        }

        try
        {
            Dictionary<string, object>? deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(await File.ReadAllTextAsync(filepath));
            if (deserialized is null)
                throw new Exception("Deserialized config was null"); //TODO: Proper exception
            properties = deserialized;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error loading config");
        }
    }

    public void Save() => File.WriteAllText(filepath, JsonConvert.SerializeObject(properties, Formatting.Indented));

    public async Task SaveAsync() => await File.WriteAllTextAsync(filepath, JsonConvert.SerializeObject(properties, Formatting.Indented));

    public void Delete()
    {
        Log.Debug("Deleting config {filepath}", filepath);
        properties.Clear();
        File.Delete(filepath);
    }
}