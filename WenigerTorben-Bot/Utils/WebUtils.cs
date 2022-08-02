using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Utils;

public class WebUtils
{

    public static async Task DownloadToDiskAsync(Uri uri, string file, HttpClient? httpClient = null)
    {
        bool noHttpClientGiven = httpClient is null;
        if (noHttpClientGiven)
            httpClient = new HttpClient();

        using FileStream fileStream = new FileStream(file, FileMode.Create);
        using Stream webStream = await httpClient.GetStreamAsync(uri);
        await webStream.CopyToAsync(fileStream);

        if (noHttpClientGiven)
            httpClient.Dispose();
    }

    public static async Task<byte[]> DownloadToRamAsync(Uri uri, HttpClient? httpClient = null)
    {
        bool noHttpClientGiven = httpClient is null;
        if (noHttpClientGiven)
            httpClient = new HttpClient();

        using MemoryStream memoryStream = new MemoryStream();
        using Stream webStream = await httpClient.GetStreamAsync(uri);
        await webStream.CopyToAsync(memoryStream);

        byte[] buffer = memoryStream.GetBuffer();

        if (noHttpClientGiven)
            httpClient.Dispose();

        return buffer;
    }

    public static async Task ImportFromWebToLibraryStorage(ILibraryStorage<byte[]>? library, string? url, string? title, string? description = null, string? tags = null, string? extras = null, Func<string, Task>? statusAction = null)
    {
        if (library is null)
            throw new ArgumentNullException(nameof(library), "Value of was null");

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url), "Value was null or an empty string");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentNullException(nameof(title), "Value was null or an empty string");

        string[]? tagsArray = null;
        Dictionary<string, string>? extrasDictionary = null;

        if (tags is not null)
            tagsArray = tags.Split(";");

        if (extras is not null)
        {
            extrasDictionary = new Dictionary<string, string>();
            foreach (string extraPair in extras.Split(";"))
            {
                string[] extraPairSplit = extraPair.Split("=");
                if (extraPairSplit.Length != 2)
                    throw new ArgumentException("Syntax for one of the defined extras contained an error", nameof(extras));

                extrasDictionary[extraPairSplit[0]] = extraPairSplit[1];
            }
        }

        if (!TryParseUri(url, out Uri? uri))
            throw new ArgumentException("Value was not a valid HTTP/-S URL", nameof(url));

        IFileService? fileService = ServiceRegistry.Get<IFileService>();
        if (fileService is null)
            throw new Exception("FileService was null"); //TODO: Proper exception

        IFFmpegService? ffmpegService = ServiceRegistry.Get<IFFmpegService>();
        if (ffmpegService is null)
            throw new Exception("FFmpegService was null"); //TODO: Proper exception

        if (fileService.Status != ServiceStatus.Started)
            throw new Exception($"FileService was not available. FileService status: {fileService.Status}."); //TODO: Proper exception

        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception($"FFmpegService was not available. FFmpegService status: {ffmpegService.Status}."); //TODO: Proper exception

        string tempFilePath = Path.Combine(fileService.GetTempDirectory(), Guid.NewGuid().ToString());

        try
        {
            if (statusAction is not null)
                await statusAction("Downloading media");
            await DownloadToDiskAsync(uri, tempFilePath);

            if (statusAction is not null)
                await statusAction("Extracting audio data from media");
            byte[] data = await ffmpegService.ReadAudioAsync(tempFilePath);

            if (data.Length == 0)
                throw new ArgumentException("The media at the given URL contained no audio to be extracted", nameof(url));
            else
            {
                await library.Import(title, description, tagsArray, extrasDictionary, data);
                if (statusAction is not null)
                    await statusAction("Audio imported to the library");
            }
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

    public static bool TryParseUri(string uriString, out Uri? uri) => Uri.TryCreate(uriString, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}