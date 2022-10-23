using System;

namespace WenigerTorbenBot.Audio.Queueing;

public class AudioRequestQueueEventArgs : EventArgs
{
    public int Position { get; init; }
    public IAudioRequest AudioRequest { get; init; }

    public AudioRequestQueueEventArgs(IAudioRequest audioRequest, int position) : base()
    {
        AudioRequest = audioRequest;
        Position = position;
    }
}