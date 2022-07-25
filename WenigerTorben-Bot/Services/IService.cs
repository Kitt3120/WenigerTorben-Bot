using System;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services;

public interface IService
{
    public string Name { get; }
    public ServicePriority Priority { get; }
    public ServiceStatus Status { get; }
    public Exception? InitializationException { get; }
    public ServiceConfiguration ServiceConfiguration { get; }

    public Task StartAsync();
    public Task StopAsync();
    public Task PostInitializeAsync();

    public bool IsAvailable();

}