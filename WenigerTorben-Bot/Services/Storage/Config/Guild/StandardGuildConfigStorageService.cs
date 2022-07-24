using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Config.Guild;

public class StandardGuildConfigStorageService<T> : BaseGuildConfigStorageService<T>
{
    public StandardGuildConfigStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string Name => "GuildConfigStorage";

    protected override string GetDefaultStorageDirectoryName() => "Guilds";
}