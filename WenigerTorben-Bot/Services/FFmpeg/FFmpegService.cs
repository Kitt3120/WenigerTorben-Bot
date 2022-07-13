using System;
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
    private string ffmpegPath;

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

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}