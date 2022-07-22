using System.Collections.Generic;
using System.Threading.Tasks;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Storage.Library;

public interface ILibraryStorage<T> : IConfigStorage<LibraryStorageEntry<T>>
{
    public Task<LibraryStorageEntry<T>?> Import(string title, string? description, string[]? tags, Dictionary<string, string>? extras, T data);
    public void Delete(string key);
    public void Delete(LibraryStorageEntry<T> libraryStorageEntry);
}