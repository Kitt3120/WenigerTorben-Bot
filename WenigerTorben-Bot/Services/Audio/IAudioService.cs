using System;
using Discord;
using WenigerTorbenBot.Audio;

namespace WenigerTorbenBot.Services.Audio;

public interface IAudioService : IService
{
    public int Enqueue(AudioRequest audioRequest);
    void Dequeue(AudioRequest audioRequest);
    void Dequeue(IGuild guild, int id);
    int GetId(AudioRequest audioRequest);
    void Pause(IGuild guild);
    void Resume(IGuild guild);
    public IAudioSession GetAudioSession(IGuild guild);
}