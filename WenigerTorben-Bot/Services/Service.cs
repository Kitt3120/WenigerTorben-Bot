using System;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services;

public abstract class Service : IService
{
    public abstract string Name { get; }
    public abstract ServicePriority Priority { get; }
    public ServiceStatus Status { get; private set; }
    public Exception? InitializationException { get; private set; }
    public Exception? DisposalException { get; private set; }
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
        {
            Serilog.Log.Warning("Service {service} tried to start multiple times", Name);
            return;
        }

        Serilog.Log.Information("Initializing service {service}", Name);
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
            Serilog.Log.Error(e, "Failed to initialize service {service}", Name);
        }
    }

    public async Task Stop()
    {
        Serilog.Log.Debug("Stopping service {service}", Name);
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
                Serilog.Log.Error(e, "Failed to dispose service {service}", Name);
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
                Serilog.Log.Error(e, "Failed to dispose service {service}", Name);
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