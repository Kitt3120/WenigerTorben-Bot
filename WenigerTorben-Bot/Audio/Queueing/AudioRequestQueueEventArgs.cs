using System;

namespace WenigerTorbenBot.Audio.Queueing;

public class AudioRequestQueueEventArgs : EventArgs
{
    public IAudioRequest AudioRequest { get; init; }

    public AudioRequestQueueEventArgs(IAudioRequest audioRequest) : base()
    {
        this.AudioRequest = audioRequest;
    }
}