using System;

namespace WenigerTorbenBot.Audio.Queueing;

public class EnqueueEventArgs : EventArgs
{
    public int Position { get; init; }
    public IAudioRequest AudioRequest { get; init; }

    public EnqueueEventArgs(IAudioRequest audioRequest, int position) : base()
    {
        AudioRequest = audioRequest;
        Position = position;
    }
}