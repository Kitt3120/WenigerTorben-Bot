using System.IO;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Audio.AudioSource;

public interface IAudioSource
{
    public void Prepare();
    public Task<Stream> ProvideAsync();
    public Task WriteToAsync(Stream stream);
    public AudioSourceType GetAudioSourceType();
}