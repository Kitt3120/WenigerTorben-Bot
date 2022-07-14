using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services.FFmpeg;

public interface IFFmpegService : IService
{
    public Process GetProcess(string filepath, params string[] arguments);
    public Task StreamAudioAsync(string filepath, Stream stream);
}