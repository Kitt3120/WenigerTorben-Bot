using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Binary;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Storage.Persistent;

public abstract class BasePersistentStorageService<T> : StorageService<T>, IPersistentStorageService<T>
{
    public BasePersistentStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override ServicePriority Priority => ServicePriority.Essential;

    protected override string GetTopLevelDirectoryName() => "BinaryStorages";

    protected override string GetStorageFileExtension() => "bin";

    public override void Load(string identifier = "global")
    {
        IStorage<T> storage = new BinaryStorage<T>(GetStorageFilePath(identifier));
        storage.Load();
        storages[identifier] = storage;
    }

}
