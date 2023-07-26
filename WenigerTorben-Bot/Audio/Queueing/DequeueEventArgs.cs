using System;

namespace WenigerTorbenBot.Audio.Queueing;

public class DequeueEventArgs : EventArgs
{
    public int Position { get; init; }
    public IAudioRequest AudioRequest { get; init; }

    public DequeueEventArgs(IAudioRequest audioRequest, int position) : base()
    {
        AudioRequest = audioRequest;
        Position = position;
    }
}