using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Audio.Source;

public class WebAudioSource : AudioSource
{
    public static bool IsApplicableFor(string request) => request.ToLower().StartsWith("http://") || request.ToLower().StartsWith("https://");

    public override AudioSourceType AudioSourceType => AudioSourceType.Web;

    public WebAudioSource(SocketGuild guild, string request) : base(guild, request)
    { }

    protected override async Task DoStreamAsync(Stream output)
    {
        IFFmpegService? ffmpegService = ServiceRegistry.Get<IFFmpegService>();
        if (ffmpegService is null)
            throw new Exception("FFmpegService was null"); //TODO: Proper exception
        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception($"FFmpegService was not available. FFmpegService status: {ffmpegService.Status}"); //TODO: Proper exception

        if (!WebUtils.TryParseUri(request, out Uri? uri) || uri is null)
            throw new ArgumentException("Value was not a valid HTTP/-S URL"); //Parameter not specified here because it could confuse users when it ends up in a message sent back to one

        await WebUtils.StreamAsync(uri, stream => ffmpegService.StreamAudioAsync(stream, output));
    }

    protected override async Task<IMetadata> DoLoadMetadataAsync()
    {
        if (!WebUtils.TryParseUri(request, out Uri? uri) || uri is null)
            throw new ArgumentException("Value was not a valid HTTP/-S URL"); //Parameter not specified here because it could confuse users when it ends up in a message sent back to one

        return new MetadataBuilder()
        .WithTitle(await WebUtils.GetTitleAsync(uri))
        .WithOrigin(uri.AbsoluteUri)
        .Build();
    }
}