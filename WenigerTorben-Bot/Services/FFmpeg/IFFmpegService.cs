using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

namespace WenigerTorbenBot.Services.FFmpeg;

public interface IFFmpegService : IService
{
    public Task StreamAudioAsync(string filepath, Stream output);
    public Task StreamAudioAsync(Stream input, Stream output);
    public Task StreamAudioAsync(StreamPipeSource input, Stream output);
    public Task<byte[]> ReadAudioAsync(string filepath);
    public Task<byte[]> ReadAudioAsync(Stream input);
    public Task<byte[]> ReadAudioAsync(StreamPipeSource input, Stream output);
}