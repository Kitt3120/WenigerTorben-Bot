using System;
using Microsoft.Extensions.DependencyInjection;
using WenigerTorbenBot.Services.Config;
using WenigerTorbenBot.Services.Health;

namespace WenigerTorbenBot;

public class DI
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Init()
    {
        IHealthService healthService = new HealthService();
        IConfigService configService = new ConfigService();

        ServiceProvider = new ServiceCollection()
        .AddSingleton(healthService)
        .AddSingleton(configService)
        .BuildServiceProvider();
    }
}