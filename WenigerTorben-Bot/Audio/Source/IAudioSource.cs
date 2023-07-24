using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Metadata;

namespace WenigerTorbenBot.Audio.Source;

public interface IAudioSource
{
    public Task WhenMetadataLoaded();
    public IMetadata GetAudioSourceMetadata();
    public void BeginPrepareContent();
    public Task WhenContentPrepared(bool autoStartContentPreparation = false);
    public Task StreamAsync(Stream output);
    public AudioSourceType GetAudioSourceType();
}