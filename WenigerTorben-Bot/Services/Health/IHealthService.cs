namespace WenigerTorbenBot.Services.Health;

public interface IHealthService
{
    public bool IsOverallHealthGood();
    public ServiceStatus? GetServiceStatus<T>();
}