using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using WenigerTorbenBot.Storage;

namespace WenigerTorbenBot.Services.Storage;

public interface IStorageService : IService, IDisposable
{
    public string GetDirectory();

    public string GetStorageFilePath(string identifier = "global");

    public IEnumerable<string> GetIdentifiers();

    public bool Exists(string identifier = "global");

    public IStorage<object>? Get(string identifier = "global");

    public void Delete(string identifier);

    public void Load(string identifier = "global");

    public void LoadAll();

    public void Save(string identifier = "global");

    public void SaveAll();

}