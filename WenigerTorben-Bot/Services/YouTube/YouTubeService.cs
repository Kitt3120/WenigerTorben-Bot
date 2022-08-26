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

    //Data
    public async Task<Video> GetVideoAsync(string urlOrId) => await youtubeClient.Videos.GetAsync(urlOrId);
    public async Task<StreamManifest> GetStreamManifestAsync(string urlOrId) => await youtubeClient.Videos.Streams.GetManifestAsync(urlOrId);
    public Task<StreamManifest> GetStreamManifestAsync(Video video) => GetStreamManifestAsync(video.Url);

    //Download
    public async Task DownloadAsync(IStreamInfo streamInfo, string filepath) => await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filepath);
    public async Task DownloadBestVideoAsync(string urlOrId, string filepath) => await DownloadAsync((await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality(), filepath);
    public Task DownloadBestVideoAsync(Video video, string filepath) => DownloadBestVideoAsync(video.Url, filepath);
    public async Task DownloadBestAudioAsync(string urlOrId, string filepath) => await DownloadAsync((await GetStreamManifestAsync(urlOrId)).GetAudioOnlyStreams().GetWithHighestBitrate(), filepath);
    public Task DownloadBestAudioAsync(Video video, string filepath) => DownloadBestAudioAsync(video.Url, filepath);
    public async Task DownloadBestMuxAsync(string urlOrId, string filepath) => await DownloadAsync((await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality(), filepath);
    public Task DownloadBestMuxAsync(Video video, string filepath) => DownloadBestMuxAsync(video.Url, filepath);

    //Stream
    public async Task StreamAsync(IStreamInfo streamInfo, Stream output) => await youtubeClient.Videos.Streams.CopyToAsync(streamInfo, output);
    public async Task StreamBestVideoAsync(string urlOrId, Stream output) => await StreamAsync((await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality(), output);
    public Task StreamBestVideoAsync(Video video, Stream output) => StreamBestVideoAsync(video.Url, output);
    public async Task StreamBestAudioAsync(string urlOrId, Stream output) => await StreamAsync((await GetStreamManifestAsync(urlOrId)).GetAudioOnlyStreams().GetWithHighestBitrate(), output);
    public Task StreamBestAudioAsync(Video video, Stream output) => StreamBestAudioAsync(video.Url, output);
    public async Task StreamBestMuxAsync(string urlOrId, Stream output) => await StreamAsync((await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality(), output);
    public Task StreamBestMuxAsync(Video video, Stream output) => StreamBestMuxAsync(video.Url, output);

    //Open
    public async Task<Stream> OpenAsync(IStreamInfo streamInfo) => await youtubeClient.Videos.Streams.GetAsync(streamInfo);
    public async Task<Stream> OpenBestVideoAsync(string urlOrId) => await OpenAsync((await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality());
    public Task<Stream> OpenBestVideoAsync(Video video) => OpenBestVideoAsync(video.Url);
    public async Task<Stream> OpenBestAudioAsync(string urlOrId) => await OpenAsync((await GetStreamManifestAsync(urlOrId)).GetAudioOnlyStreams().GetWithHighestBitrate());
    public Task<Stream> OpenBestAudioAsync(Video video) => OpenBestAudioAsync(video.Url);
    public async Task<Stream> OpenBestMuxAsync(string urlOrId) => await OpenAsync((await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality());
    public Task<Stream> OpenBestMuxAsync(Video video) => OpenBestMuxAsync(video.Url);

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}
