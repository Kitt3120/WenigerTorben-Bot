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
        ServiceProvider = new ServiceCollection()
        .AddSingleton<IHealthService>(new HealthService())
        .AddSingleton<IConfigService>(new ConfigService())
        .BuildServiceProvider();
    }
}