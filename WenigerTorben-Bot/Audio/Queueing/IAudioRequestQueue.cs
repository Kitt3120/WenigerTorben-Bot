using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Audio.Queueing;

public interface IAudioRequestQueue
{
    public int Count { get; }
    public bool IsEmpty { get; }

    public event EventHandler<EnqueueEventArgs>? OnEnqueue;
    public event EventHandler<DequeueEventArgs>? OnDequeue;
    public event EventHandler<QueueSwapEventArgs>? OnSwap;
    public event EventHandler? OnUpdate;

    public int Enqueue(IAudioRequest audioRequest, int? position = null);
    public bool Dequeue(IAudioRequest audioRequest);
    public bool Dequeue(int position);
    public bool Swap(IAudioRequest audioRequest1, IAudioRequest audioRequest2);
    public bool Swap(int position1, int position2);
    public int? GetPosition(IAudioRequest audioRequest);
    public IAudioRequest? GetAtPosition(int position);
    public IReadOnlyCollection<IAudioRequest> GetQueue();
    public IReadOnlyDictionary<int, IAudioRequest> GetQueueAsDictionary();

}