using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using WenigerTorbenBot.Services.Config;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Health;

namespace WenigerTorbenBot;

public class DI
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Init()
    {
        /*
        Services have to be instantiated manually instead of by the ServiceProvider
        because it only instantiates the Services on demand, not initially.
        This would result in the HealthService not checking for any Services on start
        because none would be registered yet.
        Sadly, there is no option to have a ServiceProvider instantiate all singletons
        intially.
        */
        HealthService healthService = new HealthService();
        FileService fileService = new FileService();
        ConfigService configService = new ConfigService(fileService);
        
        ServiceProvider = new ServiceCollection()
        .AddSingleton<IHealthService>(healthService)
        .AddSingleton<IFileService>(fileService)
        .AddSingleton<IConfigService>(configService)
        .BuildServiceProvider();

        
    }
}