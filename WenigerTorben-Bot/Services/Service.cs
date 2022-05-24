using System;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services;

public abstract class Service
{
    public abstract string Name { get; }
    public abstract ServicePriority Priority { get; }
    public ServiceStatus Status { get; private set; }
    internal Exception? InitializationException { get; private set; }

    private readonly ServiceConfiguration serviceConfiguration;

    public Service()
    {
        Status = ServiceStatus.Starting;
        serviceConfiguration = CreateServiceConfiguration();

        InitializationException = null;
        try
        {
            if (serviceConfiguration.UsesAsyncInitialization)
                InitializeAsync().GetAwaiter().GetResult();
            else
                Initialize();
            Status = ServiceStatus.Available;
        }
        catch (Exception e)
        {
            Status = ServiceStatus.Unavailable;
            InitializationException = e;
            //TODO: Proper logging
            Console.WriteLine($"Failed to initialize service {Name}: {e.Message}");
        }

        ServiceRegistry.Register(this);
    }

    public bool IsAvailable() => Status == ServiceStatus.Available;

    protected virtual void Initialize() { throw new NotImplementedException($"Initialize() has not been implemented for {Name}"); }
    protected virtual async Task InitializeAsync() { throw new NotImplementedException($"InitializeAsync() has not been implemented for {Name}"); }

    protected abstract ServiceConfiguration CreateServiceConfiguration();
}