using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord;

namespace WenigerTorbenBot.Audio;

public class AudioSession
{
    private IGuild guild;
    private object queueLock;
    private List<AudioRequest> queue;

    public AudioSession(IGuild guild)
    {
        this.guild = guild;
        this.queueLock = new object();
        this.queue = new List<AudioRequest>();
    }

    public void Enqueue(AudioRequest audioRequest)
    {
        lock (queueLock)
            if (!queue.Contains(audioRequest))
                queue.Add(audioRequest);
    }

    public void Dequeue(AudioRequest audioRequest)
    {
        lock (queueLock)
            queue.Remove(audioRequest);
    }

    public void Dequeue(int id)
    {
        lock (queueLock)
            if (id >= 0 && id < queue.Count)
                queue.RemoveAt(id);
    }

    public IReadOnlyCollection<AudioRequest> GetQueue() => queue.AsReadOnly();

    public IReadOnlyDictionary<int, AudioRequest> GetQueueAsDictionary() => queue.ToDictionary(audioRequest => queue.IndexOf(audioRequest)).ToImmutableDictionary();

}