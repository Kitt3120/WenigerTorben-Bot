using Discord;
using WenigerTorbenBot.Audio.Source;

namespace WenigerTorbenBot.Audio.Queueing;

public interface IAudioRequest
{
    public IGuildUser Requestor { get; init; }
    public IVoiceChannel VoiceChannel { get; init; }
    public ITextChannel OriginChannel { get; init; }
    public string Request { get; init; }
    public IAudioSource AudioSource { get; init; }

}