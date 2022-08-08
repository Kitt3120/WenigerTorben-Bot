using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Audio.AudioSource;

public interface IAudioSource
{
    public void BeginPrepare();
    public Task WhenPrepared();
    public IReadOnlyCollection<byte> GetData();
    public MemoryStream GetStream();
    public Task CopyToAsync(Stream stream);
    public AudioSourceType GetAudioSourceType();
}