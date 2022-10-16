using System.Collections.Generic;
using System.Threading.Tasks;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Storage.Library;

public interface ILibraryStorage<T> : IAsyncStorage<LibraryStorageEntry<T>>
{
    public Task<LibraryStorageEntry<T>?> ImportAsync(Metadata.Metadata metadata, T data, string? key = null);
    public void Delete(string key);
    public void Delete(LibraryStorageEntry<T> libraryStorageEntry);
}