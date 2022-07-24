using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Config;

public class StandardConfigStorageService<T> : BaseConfigStorageService<T>
{
    public StandardConfigStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string GetDefaultDirectory() => "Misc";
}