using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using WenigerTorbenBot.Storage;

namespace WenigerTorbenBot.Services.Storage;

public interface IStorageService<T> : IService, IDisposable
{
    public string GetStorageFilePath(string identifier = "global");

    public string GetDirectory();

    public void Load(string identifier = "global");

    public void LoadAll();

    public void Save(string identifier = "global");

    public void SaveAll();

    public IReadOnlyCollection<string> GetIdentifiers();

    public IReadOnlyCollection<IStorage<T>> GetStorages();

    public bool Exists(string identifier = "global");

    public IStorage<T>? Get(string identifier = "global");

    public void Delete(string identifier);

}