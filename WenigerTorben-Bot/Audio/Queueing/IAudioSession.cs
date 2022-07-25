using System.Collections.Generic;
using Discord;

namespace WenigerTorbenBot.Audio.Queueing;

public interface IAudioSession
{
    int Enqueue(AudioRequest audioRequest);
    void Dequeue(AudioRequest audioRequest);
    void Dequeue(int id);
    int GetId(AudioRequest audioRequest);
    void Start();
    void Pause();
    void Resume();
    void HandleQueue();
    IReadOnlyCollection<AudioRequest> GetQueue();
    IReadOnlyDictionary<int, AudioRequest> GetQueueAsDictionary();
}