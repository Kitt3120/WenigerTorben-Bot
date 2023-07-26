using System;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Player;

public interface IAudioPlayer
{
    public IGuild Guild { get; }
    public IVoiceChannel? VoiceChannel { get; }
    public AudioApplication AudioApplication { get; set; }
    public bool AutoBitrate { get; set; }
    public int Bitrate { get; set; }
    public int BufferMillis { get; set; }
    public int StepSize { get; }
    public bool Paused { get; set; }
    public Task? CurrentPlayTask { get; }
    public int? Position { get; }
    public int? Duration { get; }

    public EventHandler<FinishedEventArgs>? OnFinish { get; set; }

    public void Play(IAudioRequest audioRequest);
    public void Cancel();
}
