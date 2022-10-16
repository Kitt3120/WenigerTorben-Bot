using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Audio.AudioSource;

public class WebAudioSource : AudioSource
{
    public static bool IsApplicableFor(string request) => request.ToLower().StartsWith("http://") || request.ToLower().StartsWith("https://");

    public override AudioSourceType GetAudioSourceType() => AudioSourceType.Web;

    public WebAudioSource(SocketGuild guild, string request) : base(guild, request)
    { }

    protected override async Task DoStreamAsync(Stream output)
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

        if (!WebUtils.TryParseUri(request, out Uri? uri) || uri is null)
            throw new ArgumentException("Value was not a valid HTTP/-S URL"); //Parameter not specified here because it could confuse users when it ends up in a message sent back to one

        string tempFilePath = fileService.GetTempPath();
        try
        {
            await WebUtils.DownloadToDiskAsync(uri, tempFilePath);
            byte[] data = await ffmpegService.ReadAudioAsync(tempFilePath);

            if (data.Length == 0)
                throw new ArgumentException("The media at the given URL contained no audio to be extracted", nameof(request));

            await output.WriteAsync(data);
            await output.FlushAsync();
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