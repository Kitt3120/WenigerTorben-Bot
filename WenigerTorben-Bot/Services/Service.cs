using System;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services;

public abstract class Service : IService
{
    public abstract string Name { get; }
    public abstract ServicePriority Priority { get; }
    public ServiceStatus Status { get; protected set; }
    public Exception? InitializationException { get; protected set; }
    public Exception? DisposalException { get; protected set; }
    public ServiceConfiguration ServiceConfiguration { get; private set; }

    public Service()
    {
        Status = ServiceStatus.Stopped;
        ServiceConfiguration = CreateServiceConfiguration();
        InitializationException = null;
        DisposalException = null;
        ServiceRegistry.Register(this);
    }

    public void Start()
    {
        if (Status != ServiceStatus.Stopped)
            return;

        try
        {
            if (ServiceConfiguration.UsesAsyncInitialization)
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

    public async Task Stop()
    {
        if (this is IAsyncDisposable asyncDisposable)
        {
            try
            {
                await asyncDisposable.DisposeAsync();
            }
            catch (Exception e)
            {
                Status = ServiceStatus.Failed;
                DisposalException = e;
                //TODO: Proper logging
                Console.WriteLine($"Failed to dispose service {Name}: {e.Message}");
                return;
            }
        }
        else if (this is IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                Status = ServiceStatus.Failed;
                DisposalException = e;
                //TODO: Proper logging
                Console.WriteLine($"Failed to dispose service {Name}: {e.Message}");
                return;
            }
        }

        Status = ServiceStatus.Stopped;
    }

    public bool IsAvailable() => Status == ServiceStatus.Started;

    protected virtual void Initialize() { throw new NotImplementedException($"Initialize() has not been implemented for {Name}"); }
    protected virtual async Task InitializeAsync() { throw new NotImplementedException($"InitializeAsync() has not been implemented for {Name}"); }

    protected abstract ServiceConfiguration CreateServiceConfiguration();
}