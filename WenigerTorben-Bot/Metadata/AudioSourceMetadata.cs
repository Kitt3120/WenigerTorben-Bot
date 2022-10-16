using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Metadata;

public class AudioSourceMetadata : IAudioSourceMetadata
{
    public string ID { get; internal set; }
    public string? Title { get; internal set; }
    public string? Description { get; internal set; }
    public string? Author { get; internal set; }
    public TimeSpan Duration { get; internal set; }
    public string? Origin { get; internal set; }
    public string[]? Tags { get; internal set; }
    public Dictionary<string, string>? Extras { get; internal set; }
    public string? File { get; internal set; }

    public AudioSourceMetadata(string? id, string? title, string? description, string? author, TimeSpan? duration, string? origin, string[]? tags, Dictionary<string, string>? extras, string? file)
    {
        ID = id is null ? Guid.NewGuid().ToString() : id;
        Title = title;
        Description = description;
        Author = author;
        Duration = duration is null ? default : duration.Value;
        Origin = origin;
        Tags = tags;
        Extras = extras;
        File = file;
    }

    //Used by AudioSourceMetadataBuilder
    internal AudioSourceMetadata()
    {
        ID = Guid.NewGuid().ToString();
        Title = null;
        Description = null;
        Author = null;
        Duration = default;
        Origin = null;
        Tags = null;
        Extras = null;
        File = null;
    }
}