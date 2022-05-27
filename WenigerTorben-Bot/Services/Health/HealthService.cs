using System.Linq;

namespace WenigerTorbenBot.Services.Health;

public class HealthService : Service, IHealthService
{
    public override string Name => "Health";
    public override ServicePriority Priority => ServicePriority.Essential;

    protected override void Initialize()
    { }

    public ServiceStatus? GetServiceStatus<T>()
    {
        T? service = ServiceRegistry.Get<T>();
        if (service is not null && service is Service s)
            return s.Status;
        return null;
    }

    public bool IsOverallHealthGood() => ServiceRegistry.GetServices()
                                            .Where(service => service.Priority == ServicePriority.Essential)
                                            .All(service => service.Status == ServiceStatus.Started);

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}