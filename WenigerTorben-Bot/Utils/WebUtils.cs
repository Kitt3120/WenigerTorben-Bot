using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Utils;

public class WebUtils
{
    //TODO: When FFmpeg is usabe, create method to check media file headers for corruption. This way, the bot will be able to detect corrupt downloads or downloads of invalid files.

    public static async Task Download(Uri uri, string file, HttpClient? httpClient = null)
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

    public static async Task<byte[]> Download(Uri uri, HttpClient? httpClient = null)
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