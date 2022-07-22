using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WenigerTorbenBot.CLI;
using WenigerTorbenBot.Services.Audio;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.FancyMute;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Health;
using WenigerTorbenBot.Services.Log;
using WenigerTorbenBot.Services.Setup;
using WenigerTorbenBot.Services.Storage.Config;
using WenigerTorbenBot.Services.Storage.Library;
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
        ConfigStorageService<object> configService = new ConfigStorageService<object>(fileService);
        PersistentStorageService<object> persistentStorageService = new PersistentStorageService<object>(fileService);
        LibraryStorageService<object> libraryStorageService = new LibraryStorageService<object>(fileService);
        FFmpegService ffmpegService = new FFmpegService(fileService);
        DiscordService discordService = new DiscordService(configService);
        AudioService audioService = new AudioService(fileService, ffmpegService, discordService);
        FancyMuteService fancyMuteService = new FancyMuteService(discordService);
        SetupService setupService = new SetupService(inputHandler, configService, discordService);

        ServiceProvider = new ServiceCollection()
        .AddSingleton<IInputHandler>(inputHandler)
        .AddSingleton<IHealthService>(healthService)
        .AddSingleton<ILogService>(logService)
        .AddSingleton<IFileService>(fileService)
        .AddSingleton<IConfigStorageService<object>>(configService)
        .AddSingleton<IPersistentStorageService<object>>(persistentStorageService)
        .AddSingleton<ILibraryStorageService<object>>(libraryStorageService)
        .AddSingleton<IFFmpegService>(ffmpegService)
        .AddSingleton<IDiscordService>(discordService)
        .AddSingleton<IAudioService>(audioService)
        .AddSingleton<IFancyMuteService>(fancyMuteService)
        .AddSingleton<ISetupService>(setupService)
        .BuildServiceProvider();
    }
}