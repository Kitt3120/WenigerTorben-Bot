using System;

namespace WenigerTorbenBot.Audio.AudioSource.Metadata;

public class AudioSourceMetadataBuilder : IAudioSourceMetadataBuilder
{
    private string? title;
    private string? description;
    private string? author;
    private TimeSpan duration;
    private string? origin;

    public AudioSourceMetadataBuilder()
    {
        duration = TimeSpan.Zero;
    }

    public IAudioSourceMetadata Build()
    {
        if (title is null)
            throw new InvalidOperationException("Can't build AudioSourceMetadata because \"title\" is null");

        if (duration == TimeSpan.Zero)
            throw new InvalidOperationException("Can't build AudioSourceMetadata because \"duration\" was not set");

        if (origin is null)
            throw new InvalidOperationException("Can't build AudioSourceMetadata because \"origin\" is null");

        return new AudioSourceMetadata(title, description, author, duration, origin);
    }

    public IAudioSourceMetadataBuilder WithTitle(string title)
    {
        this.title = title;
        return this;
    }

    public IAudioSourceMetadataBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public IAudioSourceMetadataBuilder WithAuthor(string author)
    {
        this.author = author;
        return this;
    }

    public IAudioSourceMetadataBuilder WithDuration(TimeSpan duration)
    {
        this.duration = duration;
        return this;
    }

    public IAudioSourceMetadataBuilder WithOrigin(string origin)
    {
        this.origin = origin;
        return this;
    }

}