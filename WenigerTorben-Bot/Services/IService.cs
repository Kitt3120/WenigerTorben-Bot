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

    public void Start();
    public Task Stop();
    public Task Restart();

    public bool IsAvailable();

}