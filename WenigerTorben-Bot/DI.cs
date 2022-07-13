using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WenigerTorbenBot.CLI;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.FancyMute;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Health;
using WenigerTorbenBot.Services.Log;
using WenigerTorbenBot.Services.Setup;
using WenigerTorbenBot.Services.Storage.Config;
using WenigerTorbenBot.Services.Storage.Persistent;
using WenigerTorbenBot.Storage.Config;

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
        PersistentStorageService persistentStorageService = new PersistentStorageService(fileService);
        FFmpegService ffmpegService = new FFmpegService(fileService);
        DiscordService discordService = new DiscordService(configService);
        SetupService setupService = new SetupService(inputHandler, configService, discordService);
        FancyMuteService fancyMuteService = new FancyMuteService(discordService);

        ServiceProvider = new ServiceCollection()
        .AddSingleton<IInputHandler>(inputHandler)
        .AddSingleton<IHealthService>(healthService)
        .AddSingleton<ILogService>(logService)
        .AddSingleton<IFileService>(fileService)
        .AddSingleton<IConfigService>(configService)
        .AddSingleton<IPersistentStorageService>(persistentStorageService)
        .AddSingleton<IFFmpegService>(ffmpegService)
        .AddSingleton<IDiscordService>(discordService)
        .AddSingleton<ISetupService>(setupService)
        .AddSingleton<IFancyMuteService>(fancyMuteService)
        .BuildServiceProvider();
    }
}