using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.Discord;

namespace WenigerTorbenBot.Audio;

public class AudioSession
{
    public IGuild Guild { get; private set; }

    private readonly DiscordSocketClient discordSocketClient;
    private readonly object queueLock;
    private readonly List<AudioRequest> queue;
    ManualResetEvent queueThreadPause;
    private Thread? queueThread;

    public AudioSession(IDiscordService discordService, IGuild guild)
    {
        Guild = guild;
        this.discordSocketClient = discordService.GetWrappedClient();

        queueLock = new object();
        queueThreadPause = new ManualResetEvent(true);
        queue = new List<AudioRequest>();
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

            using var audioClient = targetChannel.ConnectAsync().GetAwaiter().GetResult();
            Process ffmpeg = null; //TODO: Replace with FFmpeg Service
            using var output = ffmpeg.StandardOutput.BaseStream;
            using var discord = audioClient.CreatePCMStream(AudioApplication.Music);

            try
            {
                output.CopyTo(discord);
            }
            finally
            {
                discord.Flush();
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