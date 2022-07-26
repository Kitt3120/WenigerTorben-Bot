using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Serilog;
using Serilog.Core;
using WenigerTorbenBot.Audio.AudioSource.Implementations;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.FFmpeg;

namespace WenigerTorbenBot.Audio.Queueing;

public class AudioSession : IAudioSession
{
    public IGuild Guild { get; private set; }

    private readonly IFFmpegService ffmpegService;

    private readonly object queueLock;
    private readonly List<AudioRequest> queue;
    private readonly ManualResetEvent queueThreadPause;
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
            Resume();
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
                Log.Error("Trying to handle AudioRequest in AudioSession queue for guild {guildId}, but FFmpegService has Status {ffmpegServiceStatus}.", Guild.Id, ffmpegService.Status);
                break;
            }

            IReadOnlyCollection<AudioRequest> currentQueue = GetQueue();
            if (!currentQueue.Any())
                break;

            AudioRequest audioRequest = currentQueue.First();
            IVoiceChannel? targetChannel = audioRequest.GetTargetChannel();
            if (targetChannel is null)
            {
                audioRequest.OriginChannel.SendMessageAsync($"{audioRequest.Requestor.Mention}, I was not able to determine the voice channel you're in. Your request will be skipped.").GetAwaiter().GetResult();
                Dequeue(audioRequest);
                continue;
            }


            using Stream audioStream = audioRequest.AudioSource.ProvideAsync().GetAwaiter().GetResult();
            //TODO: Error handling

            int bitrate = 96000;
            int bufferMillis = 1000;
            using IAudioClient audioClient = targetChannel.ConnectAsync().GetAwaiter().GetResult();
            using AudioOutStream voiceStream = audioClient.CreatePCMStream(AudioApplication.Music, bitrate, bufferMillis);
            try
            {
                int bufferSize = 1024 * 100; //100 KB

                byte[] buffer = new byte[bufferSize];
                int read;
                while ((read = audioStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    queueThreadPause.WaitOne(); //Pause if pause requested

                    voiceStream.Write(buffer, 0, bufferSize); //Plays stream in splitted parts, so a pause in between every part is possible
                    Thread.Sleep((bufferSize * 8) / (bitrate * bufferMillis));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while streaming audio of request {audioRequest}. Skipping entry.", audioRequest);
            }
            finally
            {
                voiceStream.Flush();
                audioClient.StopAsync().GetAwaiter().GetResult(); //Sometimes the bot does not leave voice channels when disposing audioClient. Maybe this fixes it.
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