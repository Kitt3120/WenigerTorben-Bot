using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace WenigerTorbenBot.Services.YouTube;

public interface IYouTubeService : IService
{
    //Video
    public Task<Video> GetVideoAsync(string urlOrId);
    public Task<StreamManifest> GetStreamManifestAsync(string urlOrId);
    public Task<StreamManifest> GetStreamManifestAsync(Video video);

    //Channels
    public Task<Channel> GetChannelAsync(string urlOrId);
    public Task<Channel> GetChannelByHandleAsync(ChannelHandle channelHandle);
    public Task<Channel> GetChannelBySlugAsync(ChannelSlug channelSlug);
    public Task<Channel> GetChannelByUserAsync(UserName userName);
    public IAsyncEnumerable<PlaylistVideo> GetChannelUploads(ChannelId channelId);

    //Playlists
    public Task<Playlist> GetPlaylistAsync(string urlOrId);
    public IAsyncEnumerable<Batch<PlaylistVideo>> GetPlaylistVideoBatches(PlaylistId playlistId);
    public IAsyncEnumerable<PlaylistVideo> GetPlaylistVideos(PlaylistId playlistId);

    //Search
    public IAsyncEnumerable<ISearchResult> Search(string query);
    public IAsyncEnumerable<Batch<ISearchResult>> SearchBatches(string query);
    public IAsyncEnumerable<ChannelSearchResult> SearchChannels(string query);
    public IAsyncEnumerable<PlaylistSearchResult> SearchPlaylists(string query);
    public IAsyncEnumerable<VideoSearchResult> SearchVideos(string query);

    //Open
    public Task<Stream> OpenAsync(IStreamInfo streamInfo);
    public Task<Stream> OpenBestVideoAsync(string urlOrId);
    public Task<Stream> OpenBestVideoAsync(Video video);
    public Task<Stream> OpenBestAudioAsync(string urlOrId);
    public Task<Stream> OpenBestAudioAsync(Video video);
    public Task<Stream> OpenBestMuxAsync(string urlOrId);
    public Task<Stream> OpenBestMuxAsync(Video video);

    //Stream
    public Task StreamAsync(IStreamInfo streamInfo, Stream output);
    public Task StreamBestVideoAsync(string urlOrId, Stream output);
    public Task StreamBestVideoAsync(Video video, Stream output);
    public Task StreamBestAudioAsync(string urlOrId, Stream output);
    public Task StreamBestAudioAsync(Video video, Stream output);
    public Task StreamBestMuxAsync(string urlOrId, Stream output);
    public Task StreamBestMuxAsync(Video video, Stream output);

    //Download
    public Task DownloadAsync(IStreamInfo streamInfo, string filepath);
    public Task DownloadBestVideoAsync(string urlOrId, string filepath);
    public Task DownloadBestVideoAsync(Video video, string filepath);
    public Task DownloadBestAudioAsync(string urlOrId, string filepath);
    public Task DownloadBestAudioAsync(Video video, string filepath);
    public Task DownloadBestMuxAsync(string urlOrId, string filepath);
    public Task DownloadBestMuxAsync(Video video, string filepath);
}