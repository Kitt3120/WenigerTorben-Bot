using System;

namespace WenigerTorbenBot.Audio.AudioSource.Metadata;

public interface IAudioSourceMetadata
{
    public string GetTitle();
    public string? GetDescription();
    public string? GetAuthor();
    public TimeSpan GetDuration();
    public string GetOrigin();
}