using System;
using System.Threading.Tasks;
using WenigerTorbenBot.Storage;

namespace WenigerTorbenBot.Services.Storage;

public interface IAsyncStorageService<T> : IStorageService<T>, IAsyncDisposable
{
    public new IAsyncStorage<T>? Get(string identifier = "global");

    public Task LoadAsync(string identifier = "global");

    public Task LoadAllAsync();

    public Task SaveAsync(string identifier = "global");

    public Task SaveAllAsync();

}