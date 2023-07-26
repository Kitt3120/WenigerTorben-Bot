using System;
using System.Collections.Generic;
using Discord;
using Discord.Audio;
using WenigerTorbenBot.Audio.Player;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Session;

public interface IAudioSession
{
    public IGuild Guild { get; }
    public IAudioPlayer AudioPlayer { get; }
    public IAudioRequestQueue AudioRequestQueue { get; }
    public int Position { get; set; }
    public bool HasReachedEnd { get; }
    public EventHandler<PositionChangeEventArgs>? OnPositionChange { get; set; }

    public void Pause();
    public void Resume();
    public void Previous();
    public void Skip();
}