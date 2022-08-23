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

    public static bool TryParseUri(string uriString, out Uri? uri) => Uri.TryCreate(uriString, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}