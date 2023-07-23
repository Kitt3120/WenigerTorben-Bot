using System;
using System.Collections.Generic;
using Discord;
using Discord.Audio;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Session;

public interface IAudioSession
{
    public IGuild Guild { get; }
    public AudioApplication AudioApplication { get; set; }
    public bool AutoBitrate { get; set; }
    public int Bitrate { get; set; }
    public int BufferMillis { get; set; }
    public int StepSize { get; }
    public bool Paused { get; }
    public bool HasReachedEnd { get; }
    public IAudioRequestQueue AudioRequestQueue { get; }
    public int Position { get; set; }
    public EventHandler<PositionChangeEventArgs>? OnPositionChange { get; set; }

    public void Pause();
    public void Resume();
    public void Skip();
}