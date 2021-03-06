using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Binary;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.Storage.Persistent;

public class PersistentStorageService : StorageService, IPersistentStorageService
{
    public PersistentStorageService(IFileService fileService) : base(fileService)
    { }

    public override string Name => "PersistentStorage";
    public override ServicePriority Priority => ServicePriority.Essential;

    public override string GetDirectory() => Path.Combine(fileService.GetDataDirectory(), "PersistentStorages");

    public override string GetStorageFilePath(string identifier = "global") => Path.Join(GetDirectory(), $"{identifier}.bin");

    public override void Load(string identifier = "global")
    {
        IStorage<object> storage = new BinaryStorage<object>(GetStorageFilePath(identifier));
        storage.Load();
        storages[identifier] = storage;
    }

}
