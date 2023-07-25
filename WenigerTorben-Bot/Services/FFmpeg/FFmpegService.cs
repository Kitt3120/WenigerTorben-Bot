using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using Serilog;
using Serilog.Core;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.FFmpeg;

public class FFmpegService : Service, IFFmpegService
{
    public override string Name => "FFmpeg";

    public override ServicePriority Priority => ServicePriority.Optional;

    private readonly IFileService fileService;
    private string? ffmpegPath;

    public FFmpegService(IFileService fileService)
    {
        this.fileService = fileService;
    }

    protected override void Initialize()
    {
        if (fileService.Status != ServiceStatus.Started)
            throw new Exception($"FileService is not available. FileService status: {fileService.Status}."); //TODO: Proper exception

        //Detect FFmpeg on host
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
        ffmpegPath = Path.GetDirectoryName(ffmpegPath);
        if (string.IsNullOrWhiteSpace(ffmpegPath))
            throw new FileNotFoundException("Failed to resolve FFmpeg binary's parent folder");

        //Configure FFMpegCore
        GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = ffmpegPath, TemporaryFilesFolder = fileService.GetTempDirectory() });
    }

    public async Task StreamAudioAsync(string filepath, Stream output)
    {
        await FFMpegArguments
        .FromFileInput(filepath)
        .OutputToPipe(new StreamPipeSink(output), options =>
        {
            options.WithAudioSamplingRate(48000);
            options.WithAudioBitrate(AudioQuality.Ultra);
            options.ForceFormat("s16le");
        })
        .ProcessAsynchronously();
    }

    public async Task StreamAudioAsync(Stream input, Stream output) => await StreamAudioAsync(new StreamPipeSource(input), output);

    public async Task StreamAudioAsync(StreamPipeSource input, Stream output)
    {
        await FFMpegArguments
       .FromPipeInput(input)
       .OutputToPipe(new StreamPipeSink(output), options =>
       {
           options.WithAudioSamplingRate(48000);
           options.WithAudioBitrate(AudioQuality.Ultra);
           options.ForceFormat("s16le");
       })
       .ProcessAsynchronously();
    }

    public async Task<byte[]> ReadAudioAsync(string filepath)
    {
        using MemoryStream memoryStream = new MemoryStream();
        await StreamAudioAsync(filepath, memoryStream);
        return memoryStream.GetBuffer();
    }

    public async Task<byte[]> ReadAudioAsync(Stream input)
    {
        using MemoryStream memoryStream = new MemoryStream();
        await StreamAudioAsync(input, memoryStream);
        return memoryStream.GetBuffer();
    }

    public async Task<byte[]> ReadAudioAsync(StreamPipeSource input, Stream output)
    {
        using MemoryStream memoryStream = new MemoryStream();
        await StreamAudioAsync(input, memoryStream);
        return memoryStream.GetBuffer();
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();

}