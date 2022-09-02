using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Persistent;

public class StandardPersistentStorageService : BasePersistentStorageService<object>
{
    public override string Name => "BinaryStorage";

    protected override string GetDefaultStorageDirectoryName() => "Misc";

    public StandardPersistentStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }
}