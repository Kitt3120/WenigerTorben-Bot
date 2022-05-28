using System.Collections.Generic;

namespace WenigerTorbenBot.Services;

public class ServiceRegistry
{
    private readonly static List<Service> services = new List<Service>();

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
    public static ICollection<Service> GetServices() => services.AsReadOnly();
}