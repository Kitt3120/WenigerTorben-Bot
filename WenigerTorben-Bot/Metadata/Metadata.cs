using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Metadata;

public class Metadata : IMetadata
{
    public string ID { get; internal set; }
    public string? Title { get; internal set; }
    public string? Description { get; internal set; }
    public string? Author { get; internal set; }
    public TimeSpan? Duration { get; internal set; }
    public string? Origin { get; internal set; }
    public string[]? Tags { get; internal set; }
    public Dictionary<string, string>? Extras { get; internal set; }

    public Metadata(string? id, string? title, string? description, string? author, TimeSpan? duration, string? origin, string[]? tags, Dictionary<string, string>? extras)
    {
        ID = id is null ? Guid.NewGuid().ToString() : id;
        Title = title;
        Description = description;
        Author = author;
        Duration = duration;
        Origin = origin;
        Tags = tags;
        Extras = extras;
    }

    //Used by AudioSourceMetadataBuilder
    internal Metadata()
    {
        ID = Guid.NewGuid().ToString();
        Title = null;
        Description = null;
        Author = null;
        Duration = null;
        Origin = null;
        Tags = null;
        Extras = null;
    }
}