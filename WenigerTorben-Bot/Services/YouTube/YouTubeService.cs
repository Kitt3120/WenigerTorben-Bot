using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
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

    //Video
    public async Task<Video> GetVideoAsync(string urlOrId) => await youtubeClient.Videos.GetAsync(urlOrId);
    public async Task<StreamManifest> GetStreamManifestAsync(string urlOrId) => await youtubeClient.Videos.Streams.GetManifestAsync(urlOrId);
    public Task<StreamManifest> GetStreamManifestAsync(Video video) => GetStreamManifestAsync(video.Url);

    //Channels
    public async Task<Channel> GetChannelAsync(string urlOrId) => await youtubeClient.Channels.GetAsync(urlOrId);
    public async Task<Channel> GetChannelByHandleAsync(ChannelHandle channelHandle) => await youtubeClient.Channels.GetByHandleAsync(channelHandle);
    public async Task<Channel> GetChannelBySlugAsync(ChannelSlug channelSlug) => await youtubeClient.Channels.GetBySlugAsync(channelSlug);
    public async Task<Channel> GetChannelByUserAsync(UserName userName) => await youtubeClient.Channels.GetByUserAsync(userName);
    public IAsyncEnumerable<PlaylistVideo> GetChannelUploads(ChannelId channelId) => youtubeClient.Channels.GetUploadsAsync(channelId);

    //Playlists
    public async Task<Playlist> GetPlaylistAsync(string urlOrId) => await youtubeClient.Playlists.GetAsync(urlOrId);
    public IAsyncEnumerable<Batch<PlaylistVideo>> GetPlaylistVideoBatches(PlaylistId playlistId) => youtubeClient.Playlists.GetVideoBatchesAsync(playlistId);
    public IAsyncEnumerable<PlaylistVideo> GetPlaylistVideos(PlaylistId playlistId) => youtubeClient.Playlists.GetVideosAsync(playlistId);

    //Search
    public IAsyncEnumerable<ISearchResult> Search(string query) => youtubeClient.Search.GetResultsAsync(query);
    public IAsyncEnumerable<Batch<ISearchResult>> SearchBatches(string query) => youtubeClient.Search.GetResultBatchesAsync(query);
    public IAsyncEnumerable<ChannelSearchResult> SearchChannels(string query) => youtubeClient.Search.GetChannelsAsync(query);
    public IAsyncEnumerable<PlaylistSearchResult> SearchPlaylists(string query) => youtubeClient.Search.GetPlaylistsAsync(query);
    public IAsyncEnumerable<VideoSearchResult> SearchVideos(string query) => youtubeClient.Search.GetVideosAsync(query);

    //Open
    public async Task<Stream> OpenAsync(IStreamInfo streamInfo) => await youtubeClient.Videos.Streams.GetAsync(streamInfo);
    public async Task<Stream> OpenBestVideoAsync(string urlOrId) => await OpenAsync((await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality());
    public async Task<Stream> OpenBestVideoAsync(Video video) => await OpenBestVideoAsync(video.Url);
    public async Task<Stream> OpenBestAudioAsync(string urlOrId) => await OpenAsync((await GetStreamManifestAsync(urlOrId)).GetAudioOnlyStreams().GetWithHighestBitrate());
    public async Task<Stream> OpenBestAudioAsync(Video video) => await OpenBestAudioAsync(video.Url);
    public async Task<Stream> OpenBestMuxAsync(string urlOrId) => await OpenAsync((await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality());
    public async Task<Stream> OpenBestMuxAsync(Video video) => await OpenBestMuxAsync(video.Url);

    //Stream
    public async Task StreamAsync(IStreamInfo streamInfo, Stream output) => await youtubeClient.Videos.Streams.CopyToAsync(streamInfo, output);
    public async Task StreamBestVideoAsync(string urlOrId, Stream output) => await StreamAsync((await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality(), output);
    public async Task StreamBestVideoAsync(Video video, Stream output) => await StreamBestVideoAsync(video.Url, output);
    public async Task StreamBestAudioAsync(string urlOrId, Stream output) => await StreamAsync((await GetStreamManifestAsync(urlOrId)).GetAudioOnlyStreams().GetWithHighestBitrate(), output);
    public async Task StreamBestAudioAsync(Video video, Stream output) => await StreamBestAudioAsync(video.Url, output);
    public async Task StreamBestMuxAsync(string urlOrId, Stream output) => await StreamAsync((await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality(), output);
    public async Task StreamBestMuxAsync(Video video, Stream output) => await StreamBestMuxAsync(video.Url, output);

    //Download
    public async Task DownloadAsync(IStreamInfo streamInfo, string filepath) => await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filepath);
    public async Task DownloadBestVideoAsync(string urlOrId, string filepath) => await DownloadAsync((await GetStreamManifestAsync(urlOrId)).GetVideoOnlyStreams().GetWithHighestVideoQuality(), filepath);
    public async Task DownloadBestVideoAsync(Video video, string filepath) => await DownloadBestVideoAsync(video.Url, filepath);
    public async Task DownloadBestAudioAsync(string urlOrId, string filepath) => await DownloadAsync((await GetStreamManifestAsync(urlOrId)).GetAudioOnlyStreams().GetWithHighestBitrate(), filepath);
    public async Task DownloadBestAudioAsync(Video video, string filepath) => await DownloadBestAudioAsync(video.Url, filepath);
    public async Task DownloadBestMuxAsync(string urlOrId, string filepath) => await DownloadAsync((await GetStreamManifestAsync(urlOrId)).GetMuxedStreams().GetWithHighestVideoQuality(), filepath);
    public async Task DownloadBestMuxAsync(Video video, string filepath) => await DownloadBestMuxAsync(video.Url, filepath);

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}
