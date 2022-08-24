using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.YouTube;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Audio.AudioSource;

public class YouTubeAudioSource : AudioSource
{
    public static bool IsApplicableFor(string request) => Regex.IsMatch(request, "^((?:https?:)?\\/\\/)?((?:www|m)\\.)?((?:youtube(-nocookie)?\\.com|youtu.be))(\\/(?:[\\w\\-]+\\?v=|embed\\/|v\\/)?)([\\w\\-]+)(\\S+)?$");

    public override AudioSourceType GetAudioSourceType() => AudioSourceType.YouTube;

    public YouTubeAudioSource(string request) : base(request)
    { }

    protected override async Task DoPrepareAsync()
    {
        IFileService? fileService = ServiceRegistry.Get<IFileService>();
        if (fileService is null)
            throw new Exception("FileService was null"); //TODO: Proper exception
        if (fileService.Status != ServiceStatus.Started)
            throw new Exception($"FileService was not available. FileService status: {fileService.Status}"); //TODO: Proper exception

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

        string tempFilePath = $"{fileService.GetTempPath()}.mkv";
        try
        {
            await youTubeService.DownloadBestAudioAsync(uri.AbsoluteUri, tempFilePath);
            byte[] data = await ffmpegService.ReadAudioAsync(tempFilePath);

            if (data.Length == 0)
                throw new ArgumentException("The media at the given URL contained no audio to be extracted", nameof(request));
            else
                buffer = data;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}