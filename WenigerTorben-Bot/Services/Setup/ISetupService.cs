namespace WenigerTorbenBot.Services.Setup;

public interface ISetupService : IService
{
    public bool IsSetupNeeded();
    public void BeginSetup();
    bool IsSetupRunning();
    public void Handle(string? input);
}