using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AngleSharp.Common;
using AngleSharp.Dom.Events;

namespace WenigerTorbenBot.Audio.Queueing;

public class AudioRequestQueue : IAudioRequestQueue
{
    public int Count { get => queue.Count; }

    public event EventHandler? OnEnqueue;
    public event EventHandler? OnDequeue;

    private readonly object queueLock;
    private readonly List<IAudioRequest> queue;

    public AudioRequestQueue()
    {
        queueLock = new object();
        queue = new List<IAudioRequest>();
    }

    public int Enqueue(IAudioRequest audioRequest, int? position = null)
    {
        lock (queueLock)
        {
            if (queue.Contains(audioRequest))
                throw new ArgumentException("The given IAudioRequest was already enqueued", nameof(audioRequest));

            if (position is null)
            {
                position = Count;
                queue.Add(audioRequest);
            }
            else
            {
                if (position < 0)
                    position = 0;
                else if (position > Count)
                    position = Count;

                queue.Insert(position.Value, audioRequest);
            }
        }

        OnEnqueue?.Invoke(this, new AudioRequestQueueEventArgs(audioRequest));
        return position.Value;
    }

    public bool Dequeue(IAudioRequest audioRequest)
    {
        lock (queueLock)
        {
            if (queue.Remove(audioRequest))
            {
                OnDequeue?.Invoke(this, new AudioRequestQueueEventArgs(audioRequest));
                return true;
            }
            return false;
        }
    }

    public bool Dequeue(int position)
    {
        lock (queueLock)
        {
            IAudioRequest? audioRequest = GetAtPosition(position);
            if (audioRequest is null)
                return false;

            queue.RemoveAt(position);
            OnDequeue?.Invoke(this, new AudioRequestQueueEventArgs(audioRequest));
            return true;
        }
    }

    public bool Swap(IAudioRequest audioRequest1, IAudioRequest audioRequest2)
    {
        lock (queueLock)
        {
            int? index1 = GetPosition(audioRequest1);
            int? index2 = GetPosition(audioRequest2);

            if (index1 is null || index2 is null)
                return false;

            queue[index1.Value] = audioRequest2;
            queue[index2.Value] = audioRequest1;
            return true;
        }
    }

    public bool Swap(int position1, int position2)
    {
        lock (queueLock)
        {
            IAudioRequest? audioRequest1 = GetAtPosition(position1);
            IAudioRequest? audioRequest2 = GetAtPosition(position2);

            if (audioRequest1 is null || audioRequest2 is null)
                return false;

            queue[position1] = audioRequest2;
            queue[position2] = audioRequest1;
            return true;
        }
    }

    public int? GetPosition(IAudioRequest audioRequest)
    {
        int index = queue.IndexOf(audioRequest);
        if (index == -1)
            return null;
        else
            return index;
    }

    public IAudioRequest? GetAtPosition(int position)
    {
        if (position < 0 || position >= Count)
            return null;

        return queue[position];
    }

    public IReadOnlyCollection<IAudioRequest> GetQueue() => queue.AsReadOnly();

    public IReadOnlyDictionary<int, IAudioRequest> GetQueueAsDictionary() => queue.ToDictionary(audioRequest => queue.IndexOf(audioRequest)).ToImmutableDictionary();
}