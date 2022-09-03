using System;

namespace WenigerTorbenBot.Audio.AudioSource.Metadata;

public class AudioSourceMetadata : IAudioSourceMetadata
{
    internal readonly string title;
    private readonly string? description;
    private readonly string? author;
    private readonly TimeSpan duration;
    private readonly string origin;

    public AudioSourceMetadata(string title, string? description, string? author, TimeSpan duration, string origin)
    {
        this.title = title;
        this.description = description;
        this.author = author;
        this.duration = duration;
        this.origin = origin;
    }

    public string GetTitle() => title;
    public string? GetDescription() => description;
    public string? GetAuthor() => author;
    public TimeSpan GetDuration() => duration;
    public string GetOrigin() => origin;
}