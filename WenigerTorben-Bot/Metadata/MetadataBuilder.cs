using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Metadata;

public class MetadataBuilder : IMetadataBuilder
{
    private readonly Metadata metadata;

    public MetadataBuilder()
    {
        metadata = new Metadata();
    }

    public Metadata Build() => metadata;

    public IMetadataBuilder WithID(string? id)
    {
        metadata.ID = id is null ? Guid.NewGuid().ToString() : id;
        return this;
    }

    public IMetadataBuilder WithTitle(string? title)
    {
        metadata.Title = title;
        return this;
    }

    public IMetadataBuilder WithDescription(string? description)
    {
        metadata.Description = description;
        return this;
    }

    public IMetadataBuilder WithAuthor(string? author)
    {
        metadata.Author = author;
        return this;
    }

    public IMetadataBuilder WithDuration(TimeSpan? duration)
    {
        metadata.Duration = duration;
        return this;
    }

    public IMetadataBuilder WithOrigin(string? origin)
    {
        metadata.Origin = origin;
        return this;
    }

    public IMetadataBuilder WithTags(string[]? tags)
    {
        metadata.Tags = tags;
        return this;
    }

    public IMetadataBuilder WithExtras(Dictionary<string, string>? extras)
    {
        metadata.Extras = extras;
        return this;
    }

}