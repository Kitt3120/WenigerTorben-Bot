using System;
using System.Collections.Generic;
using Discord;
using Serilog;
using WenigerTorbenBot.Audio.Queueing;
using WenigerTorbenBot.Audio.Session;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Audio;

public class AudioService : Service, IAudioService
{
    public override string Name => "Audio";

    public override ServicePriority Priority => ServicePriority.Optional;

    private readonly IFileService fileService;
    private readonly IDiscordService discordService;

    private readonly object audioSessionsLock;
    private readonly Dictionary<IGuild, IAudioSession> audioSessions;

    public AudioService(IFileService fileService, IDiscordService discordService)
    {
        this.fileService = fileService;
        this.discordService = discordService;

        this.audioSessionsLock = new object();
        this.audioSessions = new Dictionary<IGuild, IAudioSession>();
    }

    protected override void Initialize()
    {
        if (fileService.Status != ServiceStatus.Started)
            throw new Exception($"FileService is not available. FileService status: {fileService.Status}."); //TODO: Proper exception

        if (discordService.Status != ServiceStatus.Started)
            throw new Exception($"DiscordService is not available. DiscordService status: {discordService.Status}."); //TODO: Proper exception

    }

    public int Enqueue(AudioRequest audioRequest, int? position = null) => GetAudioSession(audioRequest.Requestor.Guild).AudioRequestQueue.Enqueue(audioRequest, position);

    public bool Dequeue(AudioRequest audioRequest) => GetAudioSession(audioRequest.Requestor.Guild).AudioRequestQueue.Dequeue(audioRequest);

    public bool Dequeue(IGuild guild, int position) => GetAudioSession(guild).AudioRequestQueue.Dequeue(position);

    public int? GetPosition(AudioRequest audioRequest) => GetAudioSession(audioRequest.Requestor.Guild).AudioRequestQueue.GetPosition(audioRequest);

    public void Pause(IGuild guild) => GetAudioSession(guild).Pause();

    public void Resume(IGuild guild) => GetAudioSession(guild).Resume();

    public void Previous(IGuild guild) => GetAudioSession(guild).Previous();

    public void Skip(IGuild guild) => GetAudioSession(guild).Skip();

    public IAudioSession GetAudioSession(IGuild guild)
    {
        lock (audioSessionsLock)
        {
            if (audioSessions.ContainsKey(guild))
                return audioSessions[guild];

            IAudioSession audioSession = new AudioSession(guild);
            audioSessions[guild] = audioSession;
            return audioSession;
        }
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}
