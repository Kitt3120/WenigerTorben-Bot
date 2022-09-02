using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace WenigerTorbenBot.Services.YouTube;

public interface IYouTubeService : IService
{
    //Data
    public Task<Video> GetVideoAsync(string urlOrId);
    public Task<StreamManifest> GetStreamManifestAsync(string urlOrId);
    public Task<StreamManifest> GetStreamManifestAsync(Video video);

    //Download
    public Task DownloadAsync(IStreamInfo streamInfo, string filepath);
    public Task DownloadBestVideoAsync(string urlOrId, string filepath);
    public Task DownloadBestVideoAsync(Video video, string filepath);
    public Task DownloadBestAudioAsync(string urlOrId, string filepath);
    public Task DownloadBestAudioAsync(Video video, string filepath);
    public Task DownloadBestMuxAsync(string urlOrId, string filepath);
    public Task DownloadBestMuxAsync(Video video, string filepath);

    //Stream
    public Task StreamAsync(IStreamInfo streamInfo, Stream output);
    public Task StreamBestVideoAsync(string urlOrId, Stream output);
    public Task StreamBestVideoAsync(Video video, Stream output);
    public Task StreamBestAudioAsync(string urlOrId, Stream output);
    public Task StreamBestAudioAsync(Video video, Stream output);
    public Task StreamBestMuxAsync(string urlOrId, Stream output);
    public Task StreamBestMuxAsync(Video video, Stream output);

    //Open
    public Task<Stream> OpenAsync(IStreamInfo streamInfo);
    public Task<Stream> OpenBestVideoAsync(string urlOrId);
    public Task<Stream> OpenBestVideoAsync(Video video);
    public Task<Stream> OpenBestAudioAsync(string urlOrId);
    public Task<Stream> OpenBestAudioAsync(Video video);
    public Task<Stream> OpenBestMuxAsync(string urlOrId);
    public Task<Stream> OpenBestMuxAsync(Video video);

    //TODO: Wrap methods for Search, Channels, Playlists
}