using System.Diagnostics;

namespace WenigerTorbenBot.Services.YouTube;

public interface IYouTubeService : IService
{
    public Process GetProcess(params string[] arguments);
}