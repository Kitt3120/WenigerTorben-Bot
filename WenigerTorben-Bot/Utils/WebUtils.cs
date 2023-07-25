using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WenigerTorbenBot.Audio.Source;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Utils;

public class WebUtils
{

    private static HttpClient? httpClient;

    private static HttpClient InitializeHttpClient()
    {
        httpClient ??= new HttpClient();
        return httpClient;
    }

    public static async Task DownloadToDiskAsync(Uri uri, string file, HttpClient? httpClient = null)
    {
        httpClient ??= InitializeHttpClient();

        using FileStream fileStream = new FileStream(file, FileMode.Create);
        using Stream webStream = await httpClient.GetStreamAsync(uri);
        await webStream.CopyToAsync(fileStream);
    }

    public static async Task<byte[]> DownloadToRamAsync(Uri uri, HttpClient? httpClient = null)
    {
        httpClient ??= InitializeHttpClient();

        using MemoryStream memoryStream = new MemoryStream();
        using Stream webStream = await httpClient.GetStreamAsync(uri);
        await webStream.CopyToAsync(memoryStream);

        byte[] buffer = memoryStream.GetBuffer();

        return buffer;
    }

    public static async Task DownloadToStreamAsync(Uri uri, Stream output, HttpClient? httpClient = null)
    {
        httpClient ??= InitializeHttpClient();

        using Stream webStream = await httpClient.GetStreamAsync(uri);
        await webStream.CopyToAsync(output);
    }

    public static async Task StreamAsync(Uri uri, Func<Stream, Task> action, HttpClient? httpClient = null)
    {
        httpClient ??= InitializeHttpClient();

        using Stream webStream = await httpClient.GetStreamAsync(uri);
        await action(webStream);
    }

    public static async Task<string?> GetTitleAsync(Uri uri, HttpClient? httpClient = null)
    {
        httpClient ??= InitializeHttpClient();

        string response = await httpClient.GetStringAsync(uri);
        Match match = Regex.Match(response, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase);

        string title;
        if (match.Success)
            title = match.Groups["Title"].Value;
        else
            title = "No title";

        return title;
    }

    public static bool TryParseUri(string uriString, out Uri? uri) => Uri.TryCreate(uriString, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}