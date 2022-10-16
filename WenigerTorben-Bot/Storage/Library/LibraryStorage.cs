using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Storage.Library;

public class LibraryStorage<T> : ConfigStorage<LibraryStorageEntry<T>>, ILibraryStorage<T>
{
    private readonly string directoryPath;

    //TODO: Clean on load

    public LibraryStorage(string filepath) : base(filepath)
    {
        directoryPath = Path.GetDirectoryName(filepath);
        if (directoryPath is null)
            throw new ArgumentException($"Could not get parent directory of {filepath}");

        Directory.CreateDirectory(directoryPath);
    }

    public async Task<LibraryStorageEntry<T>?> ImportAsync(Metadata.Metadata metadata, T data, string? key = null)
    {
        if (key is null)
            key = metadata.ID ?? Guid.NewGuid().ToString();

        string path = Path.Combine(directoryPath, $"{key}.bin");
        LibraryStorageEntry<T> libraryStorageEntry = new LibraryStorageEntry<T>(path, metadata);
        try
        {
            await libraryStorageEntry.WriteAsync(data);
            Set(key, libraryStorageEntry);
            return libraryStorageEntry;
        }
        catch (Exception e)
        {
            libraryStorageEntry.Delete();
            Log.Error(e, "Failed to import file {path} into LibraryStorage {libraryStorage}.", path, directoryPath);
            return null;
        }
    }

    //TODO: Wrap parent Set and Remove methods and add Save() to them. Remove Save() from Import after that.

    public override void Delete()
    {
        base.Delete();
        Directory.Delete(directoryPath);
    }

    public void Delete(string key)
    {
        LibraryStorageEntry<T>? libraryStorageEntry = Get(key);
        if (libraryStorageEntry is not null)
        {
            libraryStorageEntry.Delete();
            Remove(key);
        }
    }

    public void Delete(LibraryStorageEntry<T> libraryStorageEntry) => Delete(storage.FirstOrDefault(pair => pair.Value == libraryStorageEntry).Key);
}