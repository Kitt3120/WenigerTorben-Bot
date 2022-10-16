using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Metadata;

public interface IMetadata
{
    public string? ID { get; }
    public string? Title { get; }
    public string? Description { get; }
    public string? Author { get; }
    public TimeSpan? Duration { get; }
    public string? Origin { get; }
    public string[]? Tags { get; }
    public Dictionary<string, string>? Extras { get; }

}