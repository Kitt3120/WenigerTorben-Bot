using System.Collections.Generic;
using Discord.Audio;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Session;

public interface IAudioSession
{
    public int Enqueue(AudioRequest audioRequest);
    public void Dequeue(AudioRequest audioRequest);
    public void Dequeue(int id);
    public int GetId(AudioRequest audioRequest);
    public void Pause(bool autoPause = false);
    public void Resume();
    public void SetAudioApplication(AudioApplication audioApplication);
    public void SetBitrate(int bitrate);
    public void SetBufferMillis(int bufferMillis);
    public void HandleQueue();
    public IReadOnlyCollection<AudioRequest> GetQueue();
    public IReadOnlyDictionary<int, AudioRequest> GetQueueAsDictionary();
}