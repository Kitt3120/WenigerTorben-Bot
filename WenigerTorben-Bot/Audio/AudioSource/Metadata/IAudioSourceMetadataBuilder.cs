using System;

namespace WenigerTorbenBot.Audio.AudioSource.Metadata;

public interface IAudioSourceMetadataBuilder
{
    public IAudioSourceMetadata Build();
    public IAudioSourceMetadataBuilder WithTitle(string title);
    public IAudioSourceMetadataBuilder WithDescription(string? description);
    public IAudioSourceMetadataBuilder WithAuthor(string? author);
    public IAudioSourceMetadataBuilder WithDuration(TimeSpan duration);
    public IAudioSourceMetadataBuilder WithOrigin(string origin);
}