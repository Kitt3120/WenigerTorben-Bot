using System.Collections.Generic;

namespace WenigerTorbenBot.Services;

public class ServiceRegistry
{
    private readonly static List<IService> services = new List<IService>();

    public static void Register(Service service)
    {
        if (!services.Contains(service))
            services.Add(service);
    }

    public static T? Get<T>()
    {
        foreach (Service service in services)
            if (service is T s)
                return s;
        return default;
    }
    public static ICollection<IService> GetServices() => services.AsReadOnly();
}