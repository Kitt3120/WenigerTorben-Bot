using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AngleSharp.Common;
using AngleSharp.Dom.Events;
using Serilog;
using Serilog.Core;

namespace WenigerTorbenBot.Audio.Queueing;

public class AudioRequestQueue : IAudioRequestQueue
{
    public int Count => queue.Count;
    public bool IsEmpty => !queue.Any(); //Should be fine without lock

    public event EventHandler<QueueEventArgs>? OnEnqueue;
    public event EventHandler<QueueEventArgs>? OnDequeue;
    public event EventHandler<QueueSwapEventArgs>? OnSwap;
    public event EventHandler? OnUpdate;

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

        OnEnqueue?.Invoke(this, new QueueEventArgs(audioRequest, position.Value));
        OnUpdate?.Invoke(this, EventArgs.Empty);
        return position.Value;
    }

    public bool Dequeue(IAudioRequest audioRequest)
    {
        int? position = GetPosition(audioRequest);
        if (position is null)
            return false;

        lock (queueLock)
        {
            if (!queue.Remove(audioRequest))
            {
                Log.Warning("Could not remove audio request from queue: No element at position {Position}", position);
                return false;
            }
        }

        OnDequeue?.Invoke(this, new QueueEventArgs(audioRequest, position.Value));
        OnUpdate?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool Dequeue(int position)
    {
        IAudioRequest? audioRequest = GetAtPosition(position);
        if (audioRequest is null)
            return false;

        lock (queueLock)
        {
            try
            {
                queue.RemoveAt(position);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.Warning(e, "Could not remove audio request from queue: No element at position {Position}", position);
                return false;
            }
        }

        OnDequeue?.Invoke(this, new QueueEventArgs(audioRequest, position));
        OnUpdate?.Invoke(this, EventArgs.Empty);
        return true;
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

            OnSwap?.Invoke(this, new QueueSwapEventArgs(audioRequest1, audioRequest2, index1.Value, index2.Value));
            OnUpdate?.Invoke(this, EventArgs.Empty);
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

            OnSwap?.Invoke(this, new QueueSwapEventArgs(audioRequest1, audioRequest2, position1, position2));
            OnUpdate?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }

    //Locking even for just read operations is a good idea, check https://stackoverflow.com/a/1668984
    public int? GetPosition(IAudioRequest audioRequest)
    {
        lock (queueLock)
        {
            int index = queue.IndexOf(audioRequest);
            if (index == -1)
                return null;
            else
                return index;
        }
    }

    public IAudioRequest? GetAtPosition(int position)
    {
        lock (queueLock)
        {
            if (position < 0 || position >= Count)
                return null;

            return queue[position];
        }
    }

    //No lock needed as List.AsReadOnly() simply creates a wrapper and does not read/write any data
    public IReadOnlyCollection<IAudioRequest> GetQueue() => queue.AsReadOnly();

    public IReadOnlyDictionary<int, IAudioRequest> GetQueueAsDictionary()
    {
        lock (queueLock)
            return queue.ToDictionary(audioRequest => queue.IndexOf(audioRequest)).ToImmutableDictionary();
    }
}