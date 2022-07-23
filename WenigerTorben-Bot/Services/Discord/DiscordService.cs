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
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Services.Discord;

public class DiscordService : Service, IDiscordService
{
    public override string Name => "Discord";
    public override ServicePriority Priority => ServicePriority.Essential;

    private readonly IConfigStorageService<object> configService;

    private readonly DiscordSocketClient discordSocketClient;
    private readonly CommandService commandService;

    private IAsyncStorage<object>? storage;

    public DiscordService(IConfigStorageService<object> configService) : base()
    {
        this.configService = configService;

        this.discordSocketClient = new DiscordSocketClient(new DiscordSocketConfig()
        {
            LogLevel = LogSeverity.Info,
            AlwaysDownloadUsers = true,
            AlwaysDownloadDefaultStickers = true,
            AlwaysResolveStickers = true,
            MessageCacheSize = 50,
            GatewayIntents = GatewayIntents.All
        });

        this.commandService = new CommandService(new CommandServiceConfig()
        {
            LogLevel = LogSeverity.Info,
            CaseSensitiveCommands = false
        });
    }

    protected override async Task InitializeAsync()
    {
        storage = configService.Get();
        if (storage is null)
            throw new Exception("Global config was null");

        string? token = storage.Get<string>("discord.token");
        if (token is null)
            throw new Exception("Config is missing option discord.token"); //TODO: Proper exception


        Serilog.Log.Debug("Waiting for DiscordSocketClient to finish");
        await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), DI.ServiceProvider);
        discordSocketClient.MessageReceived += HandleMessageAsync;

        bool ready = false;
        discordSocketClient.Ready += async () => await Task.Run(() => ready = true);

        await discordSocketClient.LoginAsync(TokenType.Bot, token);
        await discordSocketClient.StartAsync();

        while (!ready)
            await Task.Delay(500);
        Serilog.Log.Debug("DiscordSocketClient is up");

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

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().SetUsesAsyncInitialization(true).Build();

    public CommandService GetCommandService() => commandService;

    public DiscordSocketClient GetWrappedClient() => discordSocketClient;
}