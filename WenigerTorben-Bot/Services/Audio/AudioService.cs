using System;
using System.Collections.Generic;
using Discord;
using WenigerTorbenBot.Audio;
using WenigerTorbenBot.Services.FFmpeg;

namespace WenigerTorbenBot.Services.Audio;

public class AudioService : Service, IAudioService
{
    public override string Name => "Audio";

    public override ServicePriority Priority => ServicePriority.Optional;

    private readonly IFFmpegService ffmpegService;

    private readonly object audioSessionsLock;
    private readonly Dictionary<IGuild, IAudioSession> audioSessions;

    public AudioService(IFFmpegService ffmpegService)
    {
        this.ffmpegService = ffmpegService;

        this.audioSessionsLock = new object();
        this.audioSessions = new Dictionary<IGuild, IAudioSession>();
    }

    protected override void Initialize()
    {
        if (ffmpegService.Status != ServiceStatus.Started)
            throw new Exception($"FFmpegService is not available. FFmpegService status: {ffmpegService.Status}."); //TODO: Proper exception
    }

    public int Enqueue(AudioRequest audioRequest) => GetAudioSession(audioRequest.Requestor.Guild).Enqueue(audioRequest);

    public void Dequeue(AudioRequest audioRequest) => GetAudioSession(audioRequest.Requestor.Guild).Dequeue(audioRequest);

    public void Dequeue(IGuild guild, int id) => GetAudioSession(guild).Dequeue(id);

    public int GetId(AudioRequest audioRequest) => GetAudioSession(audioRequest.Requestor.Guild).GetId(audioRequest);

    public void Pause(IGuild guild) => GetAudioSession(guild).Pause();

    public void Resume(IGuild guild) => GetAudioSession(guild).Resume();

    public IAudioSession GetAudioSession(IGuild guild)
    {
        lock (audioSessionsLock)
        {
            if (audioSessions.ContainsKey(guild))
                return audioSessions[guild];

            IAudioSession audioSession = new AudioSession(guild, ffmpegService);
            audioSessions[guild] = audioSession;
            return audioSession;
        }
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}
