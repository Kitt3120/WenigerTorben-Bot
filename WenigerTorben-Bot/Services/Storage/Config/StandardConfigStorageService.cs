using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Config;

public class StandardConfigStorageService : BaseConfigStorageService<object>
{
    public override string Name => "ConfigStorage";

    protected override string GetDefaultStorageDirectoryName() => "Misc";

    public StandardConfigStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }
}