using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WenigerTorbenBot.Audio.AudioSource;
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

    public static async Task ImportToLibraryStorageAsync(ILibraryStorage<byte[]>? libraryStorage, string? url, string? title, string? description = null, string? tags = null, string? extras = null)
    {
        if (libraryStorage is null)
            throw new ArgumentNullException(nameof(libraryStorage), "Value was null");

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

        WebAudioSource webAudioSource = new WebAudioSource(url);
        await webAudioSource.WhenPrepared();
        byte[] data = webAudioSource.GetData().ToArray(); //TOOD: Optimize, this currently creates a copy of the audio data in RAM
        await libraryStorage.ImportAsync(title, description, tagsArray, extrasDictionary, data);
    }

    public static bool TryParseUri(string uriString, out Uri? uri) => Uri.TryCreate(uriString, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}