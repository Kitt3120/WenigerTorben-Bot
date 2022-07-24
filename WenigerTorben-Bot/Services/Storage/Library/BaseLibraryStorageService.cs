using System.IO;
using System.Threading.Tasks;
using Serilog;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Storage.Config;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Services.Storage.Library;

public abstract class BaseLibraryStorageService<T> : AsyncStorageService<LibraryStorageEntry<T>>, ILibraryStorageService<T>
{
    public BaseLibraryStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override ServicePriority Priority => ServicePriority.Essential;

    protected override string GetTopLevelDirectoryName() => "Libraries";

    protected override string GetStorageFileExtension() => "json";

    public override string GetStorageFilePath(string identifier = "global") => Path.Join(GetDirectory(), $"{identifier}/library.{GetStorageFileExtension()}");

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