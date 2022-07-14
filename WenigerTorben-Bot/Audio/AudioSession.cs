using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.FFmpeg;

namespace WenigerTorbenBot.Audio;

public class AudioSession : IAudioSession
{
    public IGuild Guild { get; private set; }

    private readonly IFFmpegService ffmpegService;

    private readonly object queueLock;
    private readonly List<AudioRequest> queue;
    ManualResetEvent queueThreadPause;
    private Thread? queueThread;

    public AudioSession(IGuild guild, IFFmpegService ffmpegService)
    {
        Guild = guild;
        this.ffmpegService = ffmpegService;

        queueLock = new object();
        queueThreadPause = new ManualResetEvent(true);
        queue = new List<AudioRequest>();
    }

    public int Enqueue(AudioRequest audioRequest)
    {
        lock (queueLock)
            if (!queue.Contains(audioRequest))
                queue.Add(audioRequest);

        return GetId(audioRequest);
    }

    public void Dequeue(AudioRequest audioRequest)
    {
        lock (queueLock)
            queue.Remove(audioRequest);
    }

    public void Dequeue(int id)
    {
        lock (queueLock)
        {
            AudioRequest? audioRequest = queue.ElementAt(id);
            if (audioRequest is not null)
                queue.Remove(audioRequest);
        }
    }

    public int GetId(AudioRequest audioRequest)
    {
        lock (queueLock)
            return queue.IndexOf(audioRequest);
    }

    public void Start()
    {
        if (queueThread is null || !queueThread.IsAlive)
        {
            queueThread = new Thread(HandleQueue);
            queueThread.Start();
        }
        else Resume();
    }

    public void Pause() => queueThreadPause.Reset();

    public void Resume() => queueThreadPause.Set();

    public void HandleQueue()
    {
        while (true)
        {
            queueThreadPause.WaitOne();
            if (ffmpegService.Status != ServiceStatus.Started)
            {
                Serilog.Log.Error("Trying to handle AudioRequest in AudioSession queue for guild {guildId}, but FFmpegService has Status {ffmpegServiceStatus}.", Guild.Id, ffmpegService.Status);
                break;
            }

            IReadOnlyCollection<AudioRequest> currentQueue = GetQueue();
            if (!currentQueue.Any())
                break;

            AudioRequest audioRequest = currentQueue.First();
            IVoiceChannel? targetChannel = audioRequest.GetTargetChannelAsync().GetAwaiter().GetResult();
            if (targetChannel is null)
            {
                audioRequest.OriginChannel.SendMessageAsync($"{audioRequest.Requestor.Mention}, I was not able to determine the voice channel you're in. Your request will be skipped.").GetAwaiter().GetResult();
                Dequeue(audioRequest);
                continue;
            }

            using Stream audioStream = new MemoryStream(50 * 1024 * 1024); //50 MB buffer
            Task ffmpegRead = ffmpegService.StreamAudioAsync(audioRequest.Request, audioStream);

            using var audioClient = targetChannel.ConnectAsync().GetAwaiter().GetResult();
            using var discord = audioClient.CreatePCMStream(AudioApplication.Music);

            try
            {
                audioStream.CopyTo(discord);
            }
            finally
            {
                discord.Flush();
                Dequeue(audioRequest);
            }
        }
    }

    public IReadOnlyCollection<AudioRequest> GetQueue()
    {
        lock (queueLock)
            return queue.AsReadOnly();
    }

    public IReadOnlyDictionary<int, AudioRequest> GetQueueAsDictionary()
    {

        lock (queueLock)
            return queue.ToDictionary(audioRequest => queue.IndexOf(audioRequest)).ToImmutableDictionary();
    }

}