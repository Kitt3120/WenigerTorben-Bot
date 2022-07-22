using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;

namespace WenigerTorbenBot.Services.Storage;

public abstract class StorageService<T> : Service, IStorageService<T>
{
    protected IFileService fileService;

    protected Dictionary<string, IStorage<T>> storages;

    public StorageService(IFileService fileService)
    {
        this.fileService = fileService;
        this.storages = new Dictionary<string, IStorage<T>>();
    }

    protected override void Initialize()
    {
        Directory.CreateDirectory(GetDirectory());
        LoadAll();
        if (!Exists())
            Load();
    }

    public abstract string GetDirectory();

    public abstract string GetStorageFilePath(string identifier = "global");

    public IEnumerable<string> GetIdentifiers() => storages.Keys;

    public bool Exists(string identifier = "global") => storages.ContainsKey(identifier) && storages[identifier] is not null;

    public IStorage<T>? Get(string identifier = "global")
    {
        if (Exists(identifier))
            return storages[identifier];
        else return null;
    }

    public void Delete(string identifier)
    {
        storages[identifier].Delete();
        storages.Remove(identifier);
    }

    public abstract void Load(string identifier = "global");

    public void LoadAll()
    {
        foreach (string identifier in Directory.GetFiles(GetDirectory()).Select(storagePath => Path.GetFileNameWithoutExtension(storagePath)))
            Load(identifier);
    }

    public void Save(string identifier = "global") => Get(identifier)?.Save();

    public void SaveAll()
    {
        foreach (IStorage<object> storage in storages.Values)
            storage.Save();
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();

    public void Dispose() => SaveAll();
}