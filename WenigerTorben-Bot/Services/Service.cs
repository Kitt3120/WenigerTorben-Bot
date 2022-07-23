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

    public async Task StartAsync()
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
                await InitializeAsync();
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

    public async Task StopAsync()
    {
        Serilog.Log.Debug("Stopping service {service}", Name);
        if (this is IAsyncDisposable asyncDisposable)
        {
            Serilog.Log.Debug("Disposing service {service} asynchroniusly", Name);
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
            Serilog.Log.Debug("Disposing service {service}", Name);
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

    public virtual async Task PostInitializeAsync()
    {
        Serilog.Log.Debug("Running post-initialization for service {service}", Name);
        try
        {
            await DoPostInitializationAsync();
        }
        catch (Exception e)
        {
            DisposalException = e;
            Serilog.Log.Error(e, "Post-initialization failed for service {service}", Name);
            return;
        }
    }

    public bool IsAvailable() => Status == ServiceStatus.Started;

    protected virtual void Initialize() { }
    protected virtual Task InitializeAsync() { return Task.CompletedTask; }
    protected virtual Task DoPostInitializationAsync() { return Task.CompletedTask; }

    protected abstract ServiceConfiguration CreateServiceConfiguration();
}