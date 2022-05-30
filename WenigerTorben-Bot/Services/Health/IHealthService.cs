namespace WenigerTorbenBot.Services.Health;

public interface IHealthService : IService
{
    public bool IsOverallHealthGood();
    public ServiceStatus? GetServiceStatus<T>();
}