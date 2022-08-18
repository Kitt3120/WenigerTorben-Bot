using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

                path = possiblePaths.FirstOrDefault(possiblePath => System.IO.File.Exists(possiblePath));
                if (path is null)
                    throw new FileNotFoundException($"FFmpeg binary not found at {string.Join(", ", possiblePaths)}");
                ffmpegPath = path;
                break;
        }
        Serilog.Log.Debug("Using FFMpeg at {ffmpegPath}", ffmpegPath);
    }

    public Process GetProcess(params string[] arguments)
    {
        if (Status != ServiceStatus.Started)
            throw new Exception($"FFmpeg process requested but FFmpegService has Status {Status}."); //TODO: Proper exception

        Process? ffmpegProcess = Process.Start(new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = string.Join(" ", arguments),
            UseShellExecute = false,
            RedirectStandardOutput = true
        });

        if (ffmpegProcess is null)
            throw new Exception("The FFmpeg process was null."); //TODO: Proper exception

        return ffmpegProcess;
    }

    public async Task StreamAudioAsync(string filepath, Stream stream)
    {
        using Process process = GetProcess(filepath, "-hide_banner", "-loglevel panic", $"-i \"{filepath}\"", "-ac 2", "-f s16le", "-ar 48000", "pipe:1");
        await process.StandardOutput.BaseStream.CopyToAsync(stream);
    }

    public async Task<byte[]> ReadAudioAsync(string filepath)
    {
        using MemoryStream memoryStream = new MemoryStream();
        await StreamAudioAsync(filepath, memoryStream);
        return memoryStream.GetBuffer();
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();

}