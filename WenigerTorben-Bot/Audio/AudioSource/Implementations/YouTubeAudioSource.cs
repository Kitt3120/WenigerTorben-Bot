using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.YouTube;
using WenigerTorbenBot.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace WenigerTorbenBot.Audio.AudioSource;

public class YouTubeAudioSource : AudioSource
{
    public static bool IsApplicableFor(string request) => Regex.IsMatch(request, "^((?:https?:)?\\/\\/)?((?:www|music|m)\\.)?((?:youtube(-nocookie)?\\.com|youtu.be))(\\/(?:[\\w\\-]+\\?v=|embed\\/|v\\/)?)([\\w\\-]+)(\\S+)?$");

    public override AudioSourceType GetAudioSourceType() => AudioSourceType.YouTube;

    public YouTubeAudioSource(SocketGuild guild, string request) : base(guild, request)
    { }

    protected override async Task DoStreamAsync(Stream output)
    {
        IFFmpegService? ffmpegService = ServiceRegistry.Get<IFFmpegService>();
        if (ffmpegService is null)
            throw new Exception("FFmpegService was null"); //TODO: Proper exception
        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception($"FFmpegService was not available. FFmpegService status: {ffmpegService.Status}"); //TODO: Proper exception

        IYouTubeService? youTubeService = ServiceRegistry.Get<IYouTubeService>();
        if (youTubeService is null)
            throw new Exception("YouTubeService was null"); //TODO: Proper exception
        if (youTubeService.Status != ServiceStatus.Started)
            throw new Exception($"YouTubeService was not available. YouTubeService status: {youTubeService.Status}"); //TODO: Proper exception

        if (!WebUtils.TryParseUri(request, out Uri? uri) || uri is null)
            throw new ArgumentException("Value was not a valid HTTP/-S URL"); //Parameter not specified here because it could confuse users when it ends up in a message sent back to one

        using Stream youtubeStream = await youTubeService.OpenBestAudioAsync(uri.AbsoluteUri);
        await ffmpegService.StreamAudioAsync(youtubeStream, output);
    }

    protected override async Task<IAudioSourceMetadata> DoLoadMetadata()
    {
        IYouTubeService? youTubeService = ServiceRegistry.Get<IYouTubeService>();
        if (youTubeService is null)
            throw new Exception("YouTubeService was null"); //TODO: Proper exception
        if (youTubeService.Status != ServiceStatus.Started)
            throw new Exception($"YouTubeService was not available. YouTubeService status: {youTubeService.Status}"); //TODO: Proper exception

        if (!WebUtils.TryParseUri(request, out Uri? uri) || uri is null)
            throw new ArgumentException("Value was not a valid HTTP/-S URL"); //Parameter not specified here because it could confuse users when it ends up in a message sent back to one

        Video video = await youTubeService.GetVideoAsync(uri.AbsoluteUri);

        return new AudioSourceMetadataBuilder()
                .WithID(video.Id)
                .WithTitle(video.Title)
                .WithDescription(video.Description)
                .WithAuthor(video.Author.ChannelTitle)
                .WithDuration(video.Duration)
                .WithOrigin(uri.AbsoluteUri)
                .WithTags(video.Keywords.ToArray())
                .Build();
    }
}