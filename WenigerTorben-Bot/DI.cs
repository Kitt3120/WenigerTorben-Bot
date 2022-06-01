using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WenigerTorbenBot.CLI;
using WenigerTorbenBot.Services.Config;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Health;
using WenigerTorbenBot.Services.Log;
using WenigerTorbenBot.Services.Setup;

namespace WenigerTorbenBot;

public class DI
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Init()
    {
        Log.Debug("Initializing ServiceProvider");

        /*
        Services have to be instantiated manually instead of by the ServiceProvider
        because it only instantiates the Services on demand, not initially.
        This would result in the HealthService not checking for any Services on start
        because none would be registered yet.
        Sadly, there is no option to have a ServiceProvider instantiate all singletons
        initially.
        */
        InputHandler inputHandler = new InputHandler();
        HealthService healthService = new HealthService();
        FileService fileService = new FileService();
        LogService logService = new LogService(fileService);
        ConfigService configService = new ConfigService(fileService);
        DiscordService discordService = new DiscordService(configService);
        SetupService setupService = new SetupService(inputHandler, configService, discordService);

        ServiceProvider = new ServiceCollection()
        .AddSingleton<IInputHandler>(inputHandler)
        .AddSingleton<IHealthService>(healthService)
        .AddSingleton<ILogService>(logService)
        .AddSingleton<IFileService>(fileService)
        .AddSingleton<IConfigService>(configService)
        .AddSingleton<IDiscordService>(discordService)
        .AddSingleton<ISetupService>(setupService)
        .BuildServiceProvider();
    }
}