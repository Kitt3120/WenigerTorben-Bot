using System.Collections.Generic;
using Discord;

namespace WenigerTorbenBot.Audio.Queueing;

public interface IAudioSession
{
    int Enqueue(AudioRequest audioRequest);
    void Dequeue(AudioRequest audioRequest);
    void Dequeue(int id);
    int GetId(AudioRequest audioRequest);
    void Pause(bool autoPause = false);
    void Resume();
    void HandleQueue();
    IReadOnlyCollection<AudioRequest> GetQueue();
    IReadOnlyDictionary<int, AudioRequest> GetQueueAsDictionary();
}