using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Metadata;

public class AudioSourceMetadataBuilder : IAudioSourceMetadataBuilder
{
    private readonly AudioSourceMetadata audioSourceMetadata;

    public AudioSourceMetadataBuilder()
    {
        audioSourceMetadata = new AudioSourceMetadata();
    }

    public IAudioSourceMetadata Build() => audioSourceMetadata;

    public IAudioSourceMetadataBuilder WithID(string? id)
    {
        audioSourceMetadata.ID = id is null ? Guid.NewGuid().ToString() : id;
        return this;
    }

    public IAudioSourceMetadataBuilder WithTitle(string? title)
    {
        audioSourceMetadata.Title = title;
        return this;
    }

    public IAudioSourceMetadataBuilder WithDescription(string? description)
    {
        audioSourceMetadata.Description = description;
        return this;
    }

    public IAudioSourceMetadataBuilder WithAuthor(string? author)
    {
        audioSourceMetadata.Author = author;
        return this;
    }

    public IAudioSourceMetadataBuilder WithDuration(TimeSpan? duration)
    {
        audioSourceMetadata.Duration = duration;
        return this;
    }

    public IAudioSourceMetadataBuilder WithOrigin(string? origin)
    {
        audioSourceMetadata.Origin = origin;
        return this;
    }

    public IAudioSourceMetadataBuilder WithTags(string[]? tags)
    {
        audioSourceMetadata.Tags = tags;
        return this;
    }

    public IAudioSourceMetadataBuilder WithExtras(Dictionary<string, string>? extras)
    {
        audioSourceMetadata.Extras = extras;
        return this;
    }

    public IAudioSourceMetadataBuilder WithFile(string? file)
    {
        audioSourceMetadata.File = file;
        return this;
    }

}