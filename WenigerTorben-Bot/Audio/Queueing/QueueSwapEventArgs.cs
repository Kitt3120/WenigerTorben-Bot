using System;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Queueing;

public class QueueSwapEventArgs : EventArgs
{
    public int Position1 { get; init; }
    public int Position2 { get; init; }
    public IAudioRequest AudioRequest1 { get; init; }
    public IAudioRequest AudioRequest2 { get; init; }

    public QueueSwapEventArgs(IAudioRequest audioRequest1, IAudioRequest audioRequest2, int position1, int position2) : base()
    {
        AudioRequest1 = audioRequest1;
        AudioRequest2 = audioRequest2;
        Position1 = position1;
        Position2 = position2;
    }
}