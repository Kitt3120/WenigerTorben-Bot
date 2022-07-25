using System;
using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;

namespace WenigerTorbenBot.Audio.AudioSource.Implementations;

public class FileAudioSource : AudioSource
{
    public FileAudioSource(string request) : base(request)
    { }

    public static bool IsApplicableFor(string request) => File.Exists(request);

    protected override async Task DoPrepareAsync() => await Task.CompletedTask;

    protected override async Task<Stream> DoProvideAsync()
    {
        IFFmpegService? ffmpegService = ServiceRegistry.Get<IFFmpegService>();
        if (ffmpegService is null)
            throw new Exception("FFmpegService was null"); //TODO: Proper exception
        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception("FFmpegService was not available"); //TODO: Proper exception

        MemoryStream audioStream = new MemoryStream(50 * 1024 * 1024); //50 MB
        await ffmpegService.StreamAudioAsync(request, audioStream);
        audioStream.Position = 0;
        return audioStream;
    }

    public override AudioSourceType GetAudioSourceType() => AudioSourceType.File;
}