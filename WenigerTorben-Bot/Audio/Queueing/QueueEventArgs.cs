using System;

namespace WenigerTorbenBot.Audio.Queueing;

public class QueueEventArgs : EventArgs
{
    public int Position { get; init; }
    public IAudioRequest AudioRequest { get; init; }

    public QueueEventArgs(IAudioRequest audioRequest, int position) : base()
    {
        AudioRequest = audioRequest;
        Position = position;
    }
}