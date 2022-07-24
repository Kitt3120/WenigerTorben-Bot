using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Persistent;

public class StandardPersistentStorageService<T> : BasePersistentStorageService<T>
{
    public StandardPersistentStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string Name => "BinaryStorage";

    protected override string GetDefaultStorageDirectoryName() => "Misc";
}