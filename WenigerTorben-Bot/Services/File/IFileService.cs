namespace WenigerTorbenBot.Services.File;

public interface IFileService : IService
{
    public string GetDataDirectory();

    public string GetPath(params string[] paths);

    public string GetAndCreateDirectory(params string[] paths);

    public string GetAppDomainPath();
}