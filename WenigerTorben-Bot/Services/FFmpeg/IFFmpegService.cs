using System.Diagnostics;

namespace WenigerTorbenBot.Services.FFmpeg;

public interface IFFmpegService : IService
{
    public Process GetProcess(string filepath);
}