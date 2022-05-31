using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

    //TODO: Null-Check
    public void Load()
    {
        if (!File.Exists(filepath))
        {
            //TODO: Proper logging
            Console.WriteLine($"Error while loading config: File {filepath} does not exist. Creating empty config.");
            return;
        }

        Dictionary<string, object>? deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(filepath));
        if (deserialized is null)
        {
            //TODO: Proper logging and better error handling
            Console.WriteLine($"Error while loading config {filepath}: Deserialized object is null.");
            return;
        }

        properties = deserialized;
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(filepath))
        {
            //TODO: Proper logging
            Console.WriteLine($"Error while loading config: File {filepath} does not exist. Creating empty config.");
            return;
        }

        Dictionary<string, object>? deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(await File.ReadAllTextAsync(filepath));
        if (deserialized is null)
        {
            //TODO: Proper logging and better error handling
            Console.WriteLine($"Error while loading config {filepath}: Deserialized object is null.");
            return;
        }

        properties = deserialized;
    }

    public void Save() => File.WriteAllText(filepath, JsonConvert.SerializeObject(properties, Formatting.Indented));

    public async Task SaveAsync() => await System.IO.File.WriteAllTextAsync(filepath, JsonConvert.SerializeObject(properties, Formatting.Indented));

    public void Delete()
    {
        properties.Clear();
        File.Delete(filepath);
    }
}