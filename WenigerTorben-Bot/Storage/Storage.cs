using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace WenigerTorbenBot.Storage;

public abstract class Storage<T> : IStorage<T>
{
    protected readonly string filepath;

    protected Dictionary<string, T> storage;

    public Storage(string filepath)
    {
        this.filepath = filepath;

        this.storage = new Dictionary<string, T>();
    }

    public bool Exists(string key) => storage.ContainsKey(key) && storage[key] is not null;

    public T? Get(string key)
    {
        if (Exists(key))
            return storage[key];
        else return default;
    }

    public G? Get<G>(string key)
    {
        if (Exists(key) && storage[key] is G val)
            return val;
        else return default;
    }

    public T? this[string key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    public void Set(string key, T? value)
    {
        if (value is null)
            Remove(key);
        else
            storage[key] = value;
    }

    public T GetOrSet(string key, T defaultValue)
    {
        if (Exists(key))
            return Get(key) ?? defaultValue; //This should usually not return defaultValue. Exception: Heavy concurrent access
        else
        {
            Set(key, defaultValue);
            return defaultValue;
        }
    }

    public void Remove(string key) => storage.Remove(key);

    public void Delete()
    {
        Log.Debug("Deleting storage {filepath}", filepath);
        storage.Clear();
        File.Delete(filepath);
    }

    public void Load()
    {
        if (!File.Exists(filepath))
        {
            Log.Debug("No storage found at {filepath}, skipped Load()", filepath);
            return;
        }

        try
        {
            Dictionary<string, T>? loadedStorage = DoLoad();
            if (loadedStorage is null)
                throw new Exception("Deserialized storage was null"); //TODO: Proper exception
            storage = loadedStorage;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while loading storage. Keeping previous state.");
        }
    }

    public void Save()
    {
        try
        {
            DoSave();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while saving storage. Storage was not saved to disk.");
        }
    }

    protected abstract Dictionary<string, T>? DoLoad();

    protected abstract void DoSave();
}