using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using WenigerTorbenBot.Services.Config;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Services.Discord;

public class DiscordService : Service, IDiscordService
{
    public override string Name => "Discord";
    public override ServicePriority Priority => ServicePriority.Essential;

    private readonly IConfigService configService;

    private bool ready;
    private readonly DiscordSocketClient discordSocketClient;
    private readonly CommandService commandService;

    private IConfig? config;

    public DiscordService(IConfigService configService) : base()
    {
        this.configService = configService;

        this.ready = false;

        this.discordSocketClient = new DiscordSocketClient(new DiscordSocketConfig()
        {
            LogLevel = LogSeverity.Info,
            AlwaysDownloadUsers = true,
            AlwaysDownloadDefaultStickers = true,
            AlwaysResolveStickers = true,
            MessageCacheSize = 50
        });
        this.commandService = new CommandService(new CommandServiceConfig()
        {
            LogLevel = LogSeverity.Info,
            CaseSensitiveCommands = false
        });
    }

    protected override async Task InitializeAsync()
    {
        config = configService.Get();

        if (!config.Exists("discord.token"))
            throw new Exception("Config is missing option discord.token"); //TODO: Proper exception

        string token = config.Get<string>("discord.token");

        Serilog.Log.Debug("Waiting for DiscordSocketClient to finish");
        await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), DI.ServiceProvider);
        discordSocketClient.MessageReceived += HandleMessageAsync;

        discordSocketClient.Ready += OnDiscordClientReady;
        discordSocketClient.JoinedGuild += OnGuildJoinCreateConfig;
        discordSocketClient.LeftGuild += OnGuildLeftDeleteConfig;

        await discordSocketClient.LoginAsync(TokenType.Bot, token);
        await StartAsync();

        while (!ready)
            await Task.Delay(500);
        Serilog.Log.Debug("DiscordSocketClient is up");

        SynchronizeConfigs();
    }

    private void SynchronizeConfigs()
    {
        IEnumerable<string> loadedGuildIds = configService.GetGuildIds();
        IEnumerable<string> actualGuildIds = discordSocketClient.Guilds.Select(guild => Convert.ToString(guild.Id));

        IEnumerable<string> obsoleteLoadedGuildIds = loadedGuildIds.Where(guildId => !actualGuildIds.Contains(guildId));
        IEnumerable<string> newGuildIds = actualGuildIds.Where(guildId => !loadedGuildIds.Contains(guildId));

        int obsoleteAmount = obsoleteLoadedGuildIds.Count();
        int newAmount = newGuildIds.Count();

        if (obsoleteAmount > 0)
        {
            Serilog.Log.Information("Found {obsoleteAmount} obsolete guild {configOrConfigs}. Cleaning up.", obsoleteAmount, obsoleteAmount > 1 ? "configs" : "config");
            foreach (string guildId in obsoleteLoadedGuildIds)
                configService.Delete(guildId);
        }

        if (newAmount > 0)
        {
            Serilog.Log.Information("Found {newAmount} new {guildOrGuilds}. Creating {configOrConfigs}.", newAmount, newAmount > 1 ? "guilds" : "guild", newAmount > 1 ? "configs" : "config");
            foreach (string guildId in newGuildIds)
                configService.Load(guildId);
        }
    }

    private Task OnDiscordClientReady()
    {
        ready = true;
        return Task.CompletedTask;
    }

    private async Task OnGuildJoinCreateConfig(SocketGuild socketGuild)
    {
        Serilog.Log.Information("Joined Guild {guild}. Creating config.", socketGuild.Id);
        await configService.LoadAsync(Convert.ToString(socketGuild.Id));
    }
    private Task OnGuildLeftDeleteConfig(SocketGuild socketGuild)
    {
        Serilog.Log.Information("Left Guild {guild}. Deleting config.", socketGuild.Id);
        configService.Delete(Convert.ToString(socketGuild.Id));
        return Task.CompletedTask;
    }

    //Taken from https://discordnet.dev/guides/getting_started/samples/first-bot/structure.cs
    private async Task HandleMessageAsync(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage socketUserMessage || socketUserMessage.Author.Id == discordSocketClient.CurrentUser.Id || socketUserMessage.Author.IsBot)
            return;

        int pos = 0;
        if (socketUserMessage.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
        {
            SocketCommandContext socketCommandContext = new SocketCommandContext(discordSocketClient, socketUserMessage);
            IResult result = await commandService.ExecuteAsync(socketCommandContext, pos, DI.ServiceProvider);
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await socketUserMessage.Channel.SendMessageAsync($"An error occured while processing command: {result.ErrorReason}");
        }
    }

    //Proxy methods

    public ConnectionState ConnectionState => discordSocketClient.ConnectionState;

    public ISelfUser CurrentUser => discordSocketClient.CurrentUser;

    public TokenType TokenType => discordSocketClient.TokenType;

    public async Task<IReadOnlyCollection<IApplicationCommand>> BulkOverwriteGlobalApplicationCommand(ApplicationCommandProperties[] properties, RequestOptions options = null) => await discordSocketClient.BulkOverwriteGlobalApplicationCommandsAsync(properties, options);

    public async Task<IApplicationCommand> CreateGlobalApplicationCommand(ApplicationCommandProperties properties, RequestOptions options = null) => await discordSocketClient.CreateGlobalApplicationCommandAsync(properties, options);

    public async Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null) => await discordSocketClient.CreateGuildAsync(name, region, jpegIcon, options);

    public void Dispose() => discordSocketClient.Dispose();

    public async ValueTask DisposeAsync() => await discordSocketClient.DisposeAsync();

    public async Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null) => await discordSocketClient.GetApplicationInfoAsync(options);

    public async Task<BotGateway> GetBotGatewayAsync(RequestOptions options = null) => await discordSocketClient.GetBotGatewayAsync(options);

    public async Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => await discordSocketClient.GetChannelAsync(id, options);

    public async Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null) => await discordSocketClient.GetConnectionsAsync(options);

    public async Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => await Task.Run<IReadOnlyCollection<IDMChannel>>(() => discordSocketClient.DMChannels);

    public async Task<IApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions options = null) => await discordSocketClient.GetGlobalApplicationCommandAsync(id, options);

    public async Task<IReadOnlyCollection<IApplicationCommand>> GetGlobalApplicationCommandsAsync(RequestOptions options = null) => await discordSocketClient.GetGlobalApplicationCommandsAsync(options);

    public async Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => await Task.Run<IReadOnlyCollection<IGroupChannel>>(() => discordSocketClient.GroupChannels);

    public async Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => await Task.Run<IGuild>(() => discordSocketClient.GetGuild(id));

    public async Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => await Task.Run<IReadOnlyCollection<IGuild>>(() => discordSocketClient.Guilds);

    public async Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null) => await discordSocketClient.GetInviteAsync(inviteId, options);

    public async Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => await Task.Run<IReadOnlyCollection<IPrivateChannel>>(() => discordSocketClient.PrivateChannels);

    public async Task<int> GetRecommendedShardCountAsync(RequestOptions options = null) => await discordSocketClient.GetRecommendedShardCountAsync(options);

    public async Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => await discordSocketClient.GetUserAsync(id, options);

    public async Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null) => await Task.Run<IUser>(() => discordSocketClient.GetUser(username, discriminator));

    public async Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null) => await discordSocketClient.GetVoiceRegionAsync(id, options);

    public async Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null) => await discordSocketClient.GetVoiceRegionsAsync(options);

    public async Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null) => await discordSocketClient.Rest.GetWebhookAsync(id, options);

    public async Task StartAsync() => await discordSocketClient.StartAsync();

    public async Task StopAsync() => await discordSocketClient.StopAsync();

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();

    public CommandService GetCommandService() => commandService;

    public IDiscordClient GetWrappedClient() => discordSocketClient;
}