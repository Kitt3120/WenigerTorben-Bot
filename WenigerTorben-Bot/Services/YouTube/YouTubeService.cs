using System;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.YouTube;

public class YouTubeService : Service, IYouTubeService
{
    public override string Name => "YouTube";

    public override ServicePriority Priority => ServicePriority.Optional;

    private readonly IFileService fileService;
    private string? youtubeDlPath;

    public YouTubeService(IFileService fileService) : base()
    {
        this.fileService = fileService;
    }

    protected override void Initialize()
    {
        if (fileService.Status != ServiceStatus.Started)
            throw new Exception($"FileService is not available. FileService status: {fileService.Status}."); //TODO: Proper exception

        string relativePath = fileService.GetAppDomainPath();
        string? path = null;
        switch (PlatformUtils.GetOSPlatform())
        {
            case PlatformID.Win32NT:
                path = Path.Join(relativePath, "youtube-dl.exe");
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException($"YouTube-DL binary not found at {path}");
                youtubeDlPath = path;
                break;
            default:
                string[] possiblePaths = new string[] { Path.Join(relativePath, "youtube-dl"), "/bin/youtube-dl", "/usr/bin/youtube-dl", "/sbin/youtube-dl", "/usr/sbin/youtube-dl" };

                path = possiblePaths.FirstOrDefault(possiblePath => System.IO.File.Exists(possiblePath));
                if (path is null)
                    throw new FileNotFoundException($"YouTube-DL binary not found at {string.Join(", ", possiblePaths)}");
                youtubeDlPath = path;
                break;
        }
        Serilog.Log.Debug("Using YouTube-DL at {youtubeDlPath}", youtubeDlPath);
    }

    public Process GetProcess(params string[] arguments)
    {
        if (Status != ServiceStatus.Started)
            throw new Exception($"Process requested but YouTubeService has Status {Status}."); //TODO: Proper exception

        Process? youtubeDlProcess = Process.Start(new ProcessStartInfo
        {
            FileName = youtubeDlPath,
            Arguments = string.Join(" ", arguments),
            UseShellExecute = false,
            RedirectStandardOutput = true
        });

        if (youtubeDlProcess is null)
            throw new Exception("The YouTube-DL process was null."); //TODO: Proper exception

        return youtubeDlProcess;
    }

    public async Task<int> DownloadToDiskAsync(Uri uri, string filepath)
    {
        string folder = Path.GetDirectoryName(filepath);
        string extension = Path.GetExtension(filepath).Substring(1);
        string filepathWithoutExtension = Path.GetFileNameWithoutExtension(filepath);

        using Process? process = GetProcess("--geo-bypass", "--no-playlist", $"--remux-video {extension}", $"-o {Path.Combine(folder, filepathWithoutExtension)}", $"\"{uri.AbsoluteUri}\"");
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}
