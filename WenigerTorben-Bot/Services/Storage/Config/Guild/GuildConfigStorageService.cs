using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Storage.Config.Guild;

public class GuildConfigStorageService : BaseConfigStorageService<object>
{
    public override string Name => "GuildConfigStorage";

    protected override string GetDefaultStorageDirectoryName() => "Guilds";

    public GuildConfigStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    protected override Task DoPostInitializationAsync()
    {
        IDiscordService? discordService = ServiceRegistry.Get<IDiscordService>();
        if (discordService is null)
            throw new Exception("DiscordService was null"); //TODO: Proper exception
        if (discordService.Status != ServiceStatus.Started)
            throw new Exception($"DiscordService is not available. DiscordService status: {discordService.Status}."); //TODO: Proper exception

        DiscordSocketClient discordSocketClient = discordService.GetWrappedClient();
        SynchronizeConfigs(discordSocketClient.Guilds);
        discordSocketClient.JoinedGuild += OnGuildJoin;
        discordSocketClient.LeftGuild += OnGuildLeave;
        return Task.CompletedTask;
    }

    private void SynchronizeConfigs(IReadOnlyCollection<SocketGuild> guilds)
    {
        IReadOnlyCollection<string> loadedGuildIds = GetIdentifiers();
        IEnumerable<string> actualGuilds = guilds.Select(guild => guild.Id.ToString());

        IEnumerable<string> obsoleteLoadedGuildIds = loadedGuildIds.Where(guildId => guildId == "global" || !actualGuilds.Contains(guildId));
        IEnumerable<string> newGuildIds = actualGuilds.Where(guildId => !loadedGuildIds.Contains(guildId));

        int obsoleteAmount = obsoleteLoadedGuildIds.Count();
        int newAmount = newGuildIds.Count();

        if (obsoleteAmount > 0)
        {
            Serilog.Log.Information("Found {obsoleteAmount} obsolete guild {configOrConfigs}. Cleaning up.", obsoleteAmount, obsoleteAmount > 1 ? "configs" : "config");
            foreach (string guildId in obsoleteLoadedGuildIds)
                Delete(guildId);
        }

        if (newAmount > 0)
        {
            Serilog.Log.Information("Found {newAmount} new {guildOrGuilds}. Creating {configOrConfigs}.", newAmount, newAmount > 1 ? "guilds" : "guild", newAmount > 1 ? "configs" : "config");
            foreach (string guildId in newGuildIds)
                Load(guildId);
        }
    }

    private async Task OnGuildJoin(SocketGuild socketGuild)
    {
        Serilog.Log.Information("Joined Guild {guild}. Creating config.", socketGuild.Id);
        await LoadAsync(Convert.ToString(socketGuild.Id));
    }

    private Task OnGuildLeave(SocketGuild socketGuild)
    {
        Serilog.Log.Information("Left Guild {guild}. Deleting config.", socketGuild.Id);
        Delete(Convert.ToString(socketGuild.Id));
        return Task.CompletedTask;
    }
}