using System;

namespace WenigerTorbenBot.Audio.Session;

public class PositionChangeEventArgs : EventArgs
{
    public int? PositionFrom { get; init; }
    public int PositionTo { get; init; }

    public PositionChangeEventArgs(int? positionFrom, int positionTo) : base()
    {
        PositionFrom = positionFrom;
        PositionTo = positionTo;
    }
}