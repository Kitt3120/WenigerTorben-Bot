using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Binary;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Storage.Persistent;

public class PersistentStorageService<T> : StorageService<T>, IPersistentStorageService<T>
{
    public PersistentStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string Name => "PersistentStorage";
    public override ServicePriority Priority => ServicePriority.Essential;

    public override string GetDefaultDirectory() => Path.Combine(fileService.GetDataDirectory(), "PersistentStorages");

    public override string GetFileExtension() => "bin";

    public override void Load(string identifier = "global")
    {
        IStorage<T> storage = new BinaryStorage<T>(GetStorageFilePath(identifier));
        storage.Load();
        storages[identifier] = storage;
    }

}
