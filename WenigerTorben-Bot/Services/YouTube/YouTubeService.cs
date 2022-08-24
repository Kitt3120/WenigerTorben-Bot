using System;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Utils;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace WenigerTorbenBot.Services.YouTube;

public class YouTubeService : Service, IYouTubeService
{
    public override string Name => "YouTube";

    public override ServicePriority Priority => ServicePriority.Optional;

    private readonly YoutubeClient youtubeClient;

    public YouTubeService() : base()
    {
        this.youtubeClient = new YoutubeClient();
    }

    protected override void Initialize()
    { }

    public async Task<Video> GetVideoAsync(string urlOrId) => await youtubeClient.Videos.GetAsync(urlOrId);

    public async Task<StreamManifest> GetStreamManifestAsync(string urlOrId) => await youtubeClient.Videos.Streams.GetManifestAsync(urlOrId);

    public Task<StreamManifest> GetStreamManifestAsync(Video video) => GetStreamManifestAsync(video.Url);

    public async Task DownloadBestVideoAsync(string urlOrId, string filepath)
    {
        IVideoStreamInfo videoStreamInfo = (await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality();
        await youtubeClient.Videos.Streams.DownloadAsync(videoStreamInfo, filepath);
    }

    public Task DownloadBestVideoAsync(Video video, string filepath) => DownloadBestVideoAsync(video.Url, filepath);

    public async Task DownloadBestAudioAsync(string urlOrId, string filepath)
    {
        IStreamInfo audioStreamInfo = (await GetStreamManifestAsync(urlOrId)).GetAudioOnlyStreams().GetWithHighestBitrate();
        await youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, filepath);
    }

    public Task DownloadBestAudioAsync(Video video, string filepath) => DownloadBestAudioAsync(video.Url, filepath);

    public async Task DownloadBestMuxAsync(string urlOrId, string filepath)
    {
        IStreamInfo audioStreamInfo = (await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality();
        await youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, filepath);
    }

    public Task DownloadBestMuxAsync(Video video, string filepath) => DownloadBestMuxAsync(video.Url, filepath);

    public async Task DownloadAsync(IStreamInfo streamInfo, string filepath) => await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filepath);

    public async Task StreamBestVideoAsync(string urlOrId, Stream output)
    {
        IVideoStreamInfo videoStreamInfo = (await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality();
        await youtubeClient.Videos.Streams.CopyToAsync(videoStreamInfo, output);
    }

    public Task StreamBestVideoAsync(Video video, Stream output) => StreamBestVideoAsync(video.Url, output);

    public async Task StreamBestAudioAsync(string urlOrId, Stream output)
    {
        IStreamInfo audioStreamInfo = (await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality();
        await youtubeClient.Videos.Streams.CopyToAsync(audioStreamInfo, output);
    }

    public Task StreamBestAudioAsync(Video video, Stream output) => StreamBestAudioAsync(video.Url, output);

    public async Task StreamBestMuxAsync(string urlOrId, Stream output)
    {
        IStreamInfo audioStreamInfo = (await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality();
        await youtubeClient.Videos.Streams.CopyToAsync(audioStreamInfo, output);
    }

    public Task StreamBestMuxAsync(Video video, Stream output) => StreamBestMuxAsync(video.Url, output);

    public async Task StreamAsync(IStreamInfo streamInfo, Stream output) => await youtubeClient.Videos.Streams.CopyToAsync(streamInfo, output);

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}
