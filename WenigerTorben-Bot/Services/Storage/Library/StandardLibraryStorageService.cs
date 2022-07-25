using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Library;

public class StandardLibraryStorageService : BaseLibraryStorageService<object>
{
    public override string Name => "LibraryStorage";

    protected override string GetDefaultStorageDirectoryName() => "Misc";

    public StandardLibraryStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }
}