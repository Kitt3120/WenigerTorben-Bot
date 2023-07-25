using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;

namespace WenigerTorbenBot.Audio.Source.Implementations;

public class FileAudioSource : AudioSource
{
    public static bool IsApplicableFor(string request) => Path.IsPathFullyQualified(request) && File.Exists(request);

    public override AudioSourceType AudioSourceType => AudioSourceType.File;
    public FileAudioSource(SocketGuild guild, string request) : base(guild, request)
    { }


    protected override async Task DoStreamAsync(Stream output)
    {
        IFFmpegService? ffmpegService = ServiceRegistry.Get<IFFmpegService>();
        if (ffmpegService is null)
            throw new Exception("FFmpegService was null"); //TODO: Proper exception
        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception($"FFmpegService was not available. FFmpegService status: {ffmpegService.Status}"); //TODO: Proper exception

        await ffmpegService.StreamAudioAsync(request, output);
    }

    protected override Task<IMetadata> DoLoadMetadataAsync()
    {
        return Task.FromResult(new MetadataBuilder()
        .WithTitle(Path.GetFileNameWithoutExtension(request))
        .WithOrigin(request)
        .Build());
    }

}