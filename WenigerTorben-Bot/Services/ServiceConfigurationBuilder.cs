namespace WenigerTorbenBot.Services;

public class ServiceConfigurationBuilder
{
    private readonly ServiceConfiguration serviceConfiguration;

    public ServiceConfigurationBuilder()
    {
        serviceConfiguration = new ServiceConfiguration();
    }

    public ServiceConfigurationBuilder SetUsesAsyncInitialization(bool usesAsyncInitialization)
    {
        serviceConfiguration.UsesAsyncInitialization = usesAsyncInitialization;
        return this;
    }

    public ServiceConfiguration Build() => serviceConfiguration;
}