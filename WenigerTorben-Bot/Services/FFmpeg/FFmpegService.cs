using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Serilog;
using Serilog.Core;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.FFmpeg;

public class FFmpegService : Service, IFFmpegService
{
    public override string Name => "FFmpeg";

    public override ServicePriority Priority => ServicePriority.Optional;

    private IFileService fileService;
    private string? ffmpegPath;

    public FFmpegService(IFileService fileService)
    {
        this.fileService = fileService;
    }

    protected override void Initialize()
    {
        string relativePath = fileService.GetAppDomainPath();

        string? path = null;
        switch (PlatformUtils.GetOSPlatform())
        {
            case PlatformID.Win32NT:
                path = Path.Join(relativePath, "ffmpeg.exe");
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException($"FFmpeg binary not found at {path}");
                ffmpegPath = path;
                break;
            default:
                string[] possiblePaths = new string[] { Path.Join(relativePath, "ffmpeg"), "/bin/ffmpeg", "/usr/bin/ffmpeg", "/sbin/ffmpeg", "/usr/sbin/ffmpeg" };

                path = possiblePaths.First(possiblePath => System.IO.File.Exists(possiblePath));
                if (path is null)
                    throw new FileNotFoundException($"FFmpeg binary not found at {string.Join(", ", possiblePaths)}");
                ffmpegPath = path;
                break;
        }
        Serilog.Log.Debug("Using ffmpeg at {ffmpegPath}", ffmpegPath);
    }

    public Process GetProcess(string filepath)
    {
        if (Status != ServiceStatus.Started)
            throw new Exception($"Stream for file {filepath} requested but FFmpegService has Status {Status}."); //TODO: Proper exception

        if (System.IO.File.Exists(filepath))
            throw new FileNotFoundException($"No file found at {filepath}.");

        Process? ffmpegProcess = Process.Start(new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $"-hide_banner -loglevel panic -i \"{filepath}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });

        if (ffmpegProcess is null)
            throw new Exception("The FFmpeg process was null."); //TODO: Proper exception

        return ffmpegProcess;
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();

}