using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using WenigerTorbenBot.Metadata;

namespace WenigerTorbenBot.Audio.Source;

public interface IAudioSource
{
    public AudioSourceType AudioSourceType { get; }
    public Task MetadataLoadTask { get; }
    public bool MetadataLoaded { get; }
    public Task WhenMetadataLoadedAsync();
    public IMetadata? Metadata { get; }
    public Task? ContentPreparationTask { get; }
    public bool ContentPrepared { get; }
    public void PrepareContent();
    public Task WhenContentPreparedAsync(int millisecondsDelay = 1000);
    public Task StreamAsync(Stream output);
}