using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;

namespace WenigerTorbenBot.Services.Storage;

public abstract class AsyncStorageService<T> : StorageService<T>, IAsyncStorageService<T>
{

    protected AsyncStorageService(IFileService fileService) : base(fileService)
    { }

    protected override async Task InitializeAsync()
    {
        Directory.CreateDirectory(GetDirectory());
        await LoadAllAsync();
        if (!Exists())
            await LoadAsync();
    }

    IAsyncStorage<T>? IAsyncStorageService<T>.Get(string identifier)
    {
        if (Exists(identifier) && storages[identifier] is IAsyncStorage<T> storage)
            return storage;
        else return null;
    }

    public abstract Task LoadAsync(string identifier = "global");

    public async Task LoadAllAsync() => await Task.WhenAll(Directory.GetFiles(GetDirectory()).Select(storagePath => Path.GetFileNameWithoutExtension(storagePath)).Select(storagePath => LoadAsync(storagePath)));

    public async Task SaveAsync(string identifier = "global")
    {
        // => await Get(identifier)?.SaveAsync(); gives warning here
        IStorage<T>? storage = Get(identifier);
        if (storage is not null && storage is IAsyncStorage<T> asyncStorage)
            await asyncStorage.SaveAsync();

        //TODO: Log when trying to save config that does not exist
    }

    public async Task SaveAllAsync()
    {
        List<IAsyncStorage<T>> asyncStorages = new List<IAsyncStorage<T>>();
        foreach (IStorage<T> storage in storages.Values)
            if (storage is IAsyncStorage<T> asyncStorage)
                asyncStorages.Add(asyncStorage);
        await Task.WhenAll(asyncStorages.Select(storage => storage.SaveAsync()).ToArray());

    }
    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();

    public async ValueTask DisposeAsync() => await SaveAllAsync();

}
