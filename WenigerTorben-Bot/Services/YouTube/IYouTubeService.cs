using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services.YouTube;

public interface IYouTubeService : IService
{
    public Process GetProcess(params string[] arguments);
    public Task<int> DownloadToDiskAsync(Uri uri, string filepath);
}