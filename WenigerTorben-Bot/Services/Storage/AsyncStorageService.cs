using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;

namespace WenigerTorbenBot.Services.Storage;

public abstract class AsyncStorageService : StorageService, IAsyncStorageService
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

    IAsyncStorage<object>? IAsyncStorageService.Get(string identifier)
    {
        if (Exists(identifier) && storages[identifier] is IAsyncStorage<object> storage)
            return storage;
        else return null;
    }

    public abstract Task LoadAsync(string identifier = "global");

    public async Task LoadAllAsync() => await Task.WhenAll(Directory.GetFiles(GetDirectory()).Select(storagePath => Path.GetFileNameWithoutExtension(storagePath)).Select(storagePath => LoadAsync(storagePath)));

    public async Task SaveAsync(string identifier = "global")
    {
        // => await Get(identifier)?.SaveAsync(); gives warning here
        IStorage<object>? storage = Get(identifier);
        if (storage is not null && storage is IAsyncStorage<object> asyncStorage)
            await asyncStorage.SaveAsync();

        //TODO: Log when trying to save config that does not exist
    }

    public async Task SaveAllAsync()
    {
        List<IAsyncStorage<object>> asyncStorages = new List<IAsyncStorage<object>>();
        foreach (IStorage<object> storage in storages.Values)
            if (storage is IAsyncStorage<object> asyncStorage)
                asyncStorages.Add(asyncStorage);
        await Task.WhenAll(asyncStorages.Select(storage => storage.SaveAsync()).ToArray());

    }
    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();

    public async ValueTask DisposeAsync() => await SaveAllAsync();

}
