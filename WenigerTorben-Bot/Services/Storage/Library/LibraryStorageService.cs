using System.IO;
using System.Threading.Tasks;
using Serilog;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Storage.Config;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Services.Storage.Library;

public class LibraryStorageService<T> : AsyncStorageService<LibraryStorageEntry<T>>, ILibraryStorageService<T>
{
    public LibraryStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string Name => "LibraryStorage";

    public override ServicePriority Priority => ServicePriority.Essential;

    public override string GetDefaultDirectory() => Path.Combine(fileService.GetDataDirectory(), "Libraries");

    public override string GetFileExtension() => "json";

    public override string GetStorageFilePath(string identifier = "global") => Path.Join(GetDirectory(), $"{identifier}/library.{GetFileExtension()}");

    public override void Load(string identifier = "global")
    {
        IAsyncStorage<LibraryStorageEntry<T>> storage = new LibraryStorage<T>(GetStorageFilePath(identifier));
        storage.Load();
        storages[identifier] = storage;
    }

    public override async Task LoadAsync(string identifier = "global")
    {
        IAsyncStorage<LibraryStorageEntry<T>> storage = new LibraryStorage<T>(GetStorageFilePath(identifier));
        await storage.LoadAsync();
        storages[identifier] = storage;
    }
}