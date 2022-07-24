using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Library;

public class StandardLibraryStorageService<T> : BaseLibraryStorageService<T>
{
    public StandardLibraryStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string Name => "LibraryStorage";

    protected override string GetDefaultStorageDirectoryName() => "Libraries";
}