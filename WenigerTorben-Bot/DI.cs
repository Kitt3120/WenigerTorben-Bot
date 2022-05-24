using System;
using Microsoft.Extensions.DependencyInjection;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot;

public class DI
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Init()
    {
        ServiceProvider = new ServiceCollection()
        .AddSingleton<IConfig, Config>()
        .BuildServiceProvider();
    }
}