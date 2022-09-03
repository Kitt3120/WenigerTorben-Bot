using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Services.FFmpeg;

public interface IFFmpegService : IService
{
    public Task StreamAudioAsync(string filepath, Stream output);
    public Task StreamAudioAsync(Stream input, Stream output);
    public Task<byte[]> ReadAudioAsync(string filepath);
    public Task<byte[]> ReadAudioAsync(Stream input);
}