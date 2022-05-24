using System;

namespace WenigerTorbenBot.Services;

public abstract class Service
{
    public string Name { get; private set; }
    public ServiceStatus Status { get; private set; }
    public Service(string name)
    {
        Name = name;
        Status = ServiceStatus.Starting;
        try
        {
            Initialize();
            Status = ServiceStatus.Available;
        }
        catch (Exception e)
        {
            Status = ServiceStatus.Unavailable;
            //TODO: Proper logging
            Console.WriteLine($"Failed to initialize service {Name}: {e.Message}");
        }
    }

    public bool IsAvailable() => Status == ServiceStatus.Available;

    protected abstract void Initialize();
}