using System;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services;

public abstract class Service
{
    public abstract string Name { get; }
    public abstract ServicePriority Priority { get; }
    public ServiceStatus Status { get; protected set; }
    public Exception? InitializationException { get; protected set; }

    private readonly ServiceConfiguration serviceConfiguration;

    public Service()
    {
        Status = ServiceStatus.Stopped;
        serviceConfiguration = CreateServiceConfiguration();
        InitializationException = null;
        ServiceRegistry.Register(this);
    }

    public void Start()
    {
        if(Status != ServiceStatus.Stopped)
            return;

        try
        {
            if (serviceConfiguration.UsesAsyncInitialization)
                InitializeAsync().GetAwaiter().GetResult();
            else
                Initialize();
            Status = ServiceStatus.Started;
        }
        catch (Exception e)
        {
            Status = ServiceStatus.Failed;
            InitializationException = e;
            //TODO: Proper logging
            Console.WriteLine($"Failed to initialize service {Name}: {e.Message}");
        }
    }

    public void Stop()
    {
        if(this is IDisposable disposable)
            disposable.Dispose();

        Status = ServiceStatus.Stopped;
    }

    public bool IsAvailable() => Status == ServiceStatus.Started;

    protected virtual void Initialize() { throw new NotImplementedException($"Initialize() has not been implemented for {Name}"); }
    protected virtual async Task InitializeAsync() { throw new NotImplementedException($"InitializeAsync() has not been implemented for {Name}"); }

    protected abstract ServiceConfiguration CreateServiceConfiguration();
}