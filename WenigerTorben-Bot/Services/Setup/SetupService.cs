using System;
using System.Collections;
using System.IO;
using System.Linq;
using WenigerTorbenBot.CLI;
using WenigerTorbenBot.Services.Config;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Services.Setup;

public class SetupService : Service, ISetupService
{
    public override string Name => "Setup";
    public override ServicePriority Priority => ServicePriority.Optional;

    private readonly InputHandler inputHandler;
    private readonly IConfigService configService;
    private readonly IDiscordService discordService;

    private IConfig? config;
    private readonly string[] neededKeys;
    private int id;
    private int state;
    private readonly int endState;
    private bool running;

    public SetupService(InputHandler inputHandler, IConfigService configService, IDiscordService discordService)
    {
        this.inputHandler = inputHandler;
        this.configService = configService;
        this.discordService = discordService;

        neededKeys = new string[] { "discord.token" };
        state = 0;
        endState = 1;
        running = false;
    }

    protected override void Initialize()
    {
        config = configService.Get();
    }

    public bool IsSetupNeeded() => neededKeys.Any(key => !config.Exists(key));

    public void BeginSetup()
    {
        Console.WriteLine("Your config is missing some important settings. Entering setup...");
        id = inputHandler.RegisterInterrupt(Handle);
        running = true;
        PrintPrompt();
    }

    public bool IsSetupRunning() => running;

    private void PrintPrompt()
    {
        switch (state)
        {
            case 0:
                Console.Write("Please provide a Discord token: ");
                break;
            default:
                Console.WriteLine("Invalid state"); //TODO: Proper logging
                break;
        }
    }

    public void Handle(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        switch (state)
        {
            case 0:
                config["discord.token"] = input;
                state++;
                break;
            default:
                Console.WriteLine("Invalid state"); //TODO: Proper logging
                break;
        }

        if (state == endState)
        {
            inputHandler.ReleaseInterrupt(id);
            running = false;
            Console.WriteLine("Setup complete");
        }
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();

}