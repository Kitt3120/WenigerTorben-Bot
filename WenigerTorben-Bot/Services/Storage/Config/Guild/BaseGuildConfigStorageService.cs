using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Storage.Config;

namespace WenigerTorbenBot.Services.Storage.Config.Guild;

public abstract class BaseGuildConfigStorageService<T> : BaseConfigStorageService<T>, IGuildConfigStorageService<T>
{
    public BaseGuildConfigStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    protected override Task DoPostInitializationAsync()
    {
        IDiscordService? discordService = ServiceRegistry.Get<IDiscordService>();
        if (discordService is null)
            throw new Exception("DiscordService was null"); //TODO: Proper exception
        if (discordService.Status != ServiceStatus.Started)
            throw new Exception($"DiscordService is not available. DiscordService status: {discordService.Status}."); //TODO: Proper exception

        DiscordSocketClient discordSocketClient = discordService.GetWrappedClient();
        (this as IGuildConfigStorageService<T>).SynchronizeConfigs(discordSocketClient);
        discordSocketClient.JoinedGuild += (this as IGuildConfigStorageService<T>).OnGuildJoin;
        discordSocketClient.LeftGuild += (this as IGuildConfigStorageService<T>).OnGuildLeave;
        return Task.CompletedTask;
    }

    void IGuildConfigStorageService<T>.SynchronizeConfigs(DiscordSocketClient discordSocketClient)
    {
        IEnumerable<string> loadedGuildIds = GetIdentifiers();
        IEnumerable<string> actualGuildIds = discordSocketClient.Guilds.Select(guild => Convert.ToString(guild.Id));

        IEnumerable<string> obsoleteLoadedGuildIds = loadedGuildIds.Where(guildId => !actualGuildIds.Contains(guildId));
        IEnumerable<string> newGuildIds = actualGuildIds.Where(guildId => !loadedGuildIds.Contains(guildId));

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

    async Task IGuildConfigStorageService<T>.OnGuildJoin(SocketGuild socketGuild)
    {
        Serilog.Log.Information("Joined Guild {guild}. Creating config.", socketGuild.Id);
        await LoadAsync(Convert.ToString(socketGuild.Id));
    }

    Task IGuildConfigStorageService<T>.OnGuildLeave(SocketGuild socketGuild)
    {
        Serilog.Log.Information("Left Guild {guild}. Deleting config.", socketGuild.Id);
        Delete(Convert.ToString(socketGuild.Id));
        return Task.CompletedTask;
    }
}