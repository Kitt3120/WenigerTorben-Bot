using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;

namespace WenigerTorbenBot.Audio.AudioSource.Implementations;

public class FileAudioSource : AudioSource
{
    public static bool IsApplicableFor(string request) => Path.IsPathFullyQualified(request) && File.Exists(request);

    public FileAudioSource(string request) : base(request)
    { }

    public override AudioSourceType GetAudioSourceType() => AudioSourceType.File;

    protected override async Task DoPrepareAsync()
    {
        IFFmpegService? ffmpegService = ServiceRegistry.Get<IFFmpegService>();
        if (ffmpegService is null)
            throw new Exception("FFmpegService was null"); //TODO: Proper exception
        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception($"FFmpegService was not available. FFmpegService status: {ffmpegService.Status}"); //TODO: Proper exception

        buffer = await ffmpegService.ReadAudioAsync(request);
    }

}