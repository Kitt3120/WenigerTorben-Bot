using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Audio.AudioSource.Metadata;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;

namespace WenigerTorbenBot.Audio.AudioSource.Implementations;

public class FileAudioSource : AudioSource
{
    public static bool IsApplicableFor(string request) => Path.IsPathFullyQualified(request) && File.Exists(request);

    public override AudioSourceType GetAudioSourceType() => AudioSourceType.File;

    public FileAudioSource(SocketGuild guild, string request) : base(guild, request)
    { }


    protected override async Task DoStreamAsync(Stream output)
    {
        IFFmpegService? ffmpegService = ServiceRegistry.Get<IFFmpegService>();
        if (ffmpegService is null)
            throw new Exception("FFmpegService was null"); //TODO: Proper exception
        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception($"FFmpegService was not available. FFmpegService status: {ffmpegService.Status}"); //TODO: Proper exception

        byte[] data = await ffmpegService.ReadAudioAsync(request);
        if (data.Length == 0)
            throw new ArgumentException("The media at the given path contained no audio to be extracted", nameof(request));

        await output.WriteAsync(await ffmpegService.ReadAudioAsync(request));
        await output.FlushAsync();
    }

    protected override Task<IAudioSourceMetadata> DoLoadMetadata()
    {
        return Task.FromResult(new AudioSourceMetadataBuilder()
        .WithTitle("title")
        .WithDuration(TimeSpan.FromSeconds(1))
        .WithOrigin("file")
        .Build());
    }

}