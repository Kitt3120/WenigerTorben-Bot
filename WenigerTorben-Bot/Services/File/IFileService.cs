namespace WenigerTorbenBot.Services.File;

public interface IFileService
{
    public string DataPath { get; }
    public string ConfigPath { get; }
    
    public string GetConfigDirectory();

    public string GetPath(params string[] paths);

    public string GetAndCreateDirectory(params string[] paths);
}