using Discord;
using WenigerTorbenBot.Audio.Queueing;
using WenigerTorbenBot.Audio.Session;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Services.Audio;

public interface IAudioService : IService
{
    public int Enqueue(AudioRequest audioRequest);
    public void Dequeue(AudioRequest audioRequest);
    public void Dequeue(IGuild guild, int id);
    public int? GetPosition(AudioRequest audioRequest);
    public void Pause(IGuild guild);
    public void Resume(IGuild guild);
    public void Skip(IGuild guild);
    public IAudioSession GetAudioSession(IGuild guild);
}