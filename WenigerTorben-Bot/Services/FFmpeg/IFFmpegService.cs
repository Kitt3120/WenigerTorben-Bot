using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services.FFmpeg;

public interface IFFmpegService : IService
{
    public Process GetProcess(params string[] arguments);
    public Task StreamAudioAsync(string filepath, Stream stream);
    public Task<byte[]> ReadAudioAsync(string filepath);
}