using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
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

    private readonly object queueLock;
    private readonly List<AudioRequest> queue;
    private readonly ManualResetEvent pauseEvent;
    private readonly object autoPauseLock;
    private bool autoPaused;
    private readonly Thread queueThread;

    public AudioSession(IGuild guild)
    {
        Guild = guild;

        queueLock = new object();
        queue = new List<AudioRequest>();
        pauseEvent = new ManualResetEvent(false);
        autoPauseLock = new object();
        autoPaused = true;
        queueThread = new Thread(HandleQueue);
        queueThread.Start();
    }

    public int Enqueue(AudioRequest audioRequest)
    {
        lock (queueLock)
            if (!queue.Contains(audioRequest))
                queue.Add(audioRequest);

        bool wasAutoPaused;
        lock (autoPauseLock)
            wasAutoPaused = autoPaused;

        if (wasAutoPaused)
            Resume();

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

    public void Pause(bool autoPause = false)
    {
        lock (autoPauseLock)
        {
            autoPaused = autoPause;
            pauseEvent.Reset();
        }
    }

    public void Resume()
    {
        lock (autoPauseLock)
        {
            autoPaused = false;
            pauseEvent.Set();
        }
    }

    public async void HandleQueue()
    {
        while (true)
        {
            pauseEvent.WaitOne();

            IReadOnlyCollection<AudioRequest> currentQueue = GetQueue();
            if (!currentQueue.Any())
            {
                Pause(true);
                continue;
            }

            AudioRequest audioRequest = currentQueue.First();
            IVoiceChannel? targetChannel = audioRequest.GetTargetChannel();
            if (targetChannel is null)
            {
                await audioRequest.OriginChannel.SendMessageAsync($"Sorry {audioRequest.Requestor.Mention}, I was not able to determine the voice channel you're in. Your request will be skipped.");
                Dequeue(audioRequest);
                continue;
            }

            try
            {
                await audioRequest.AudioSource.WhenPrepared();
            }
            catch (Exception e)
            {
                if (e is not ArgumentException && e is not HttpRequestException)
                    Log.Error(e, "An error occured while preparing an AudioSource.\nRequest: {request}.", audioRequest.Request);

                await audioRequest.OriginChannel.SendMessageAsync($"Sorry {audioRequest.Requestor.Mention}, there was an error while playing your requested audio: {e.Message}.\nYour request will be skipped.");
                Dequeue(audioRequest);
                continue;
            }

            using MemoryStream audioStream = audioRequest.AudioSource.CreateStream();

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
                    pauseEvent.WaitOne();
                    voiceStream.Write(buffer, 0, bufferSize); //Plays stream in splitted parts, so a pause in between every part is possible
                    Thread.Sleep((bufferSize * 8) / (bitrate * bufferMillis));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while streaming audio of request {audioRequest} to channel {channel} in guild {guild}. Skipping entry.", audioRequest.Request, audioRequest.GetTargetChannel(), audioRequest.GetTargetChannel().Guild.Id);
                await audioRequest.OriginChannel.SendMessageAsync($"{audioRequest.Requestor.Mention}, an error occured while playing your requested media: {e.Message}\nYour request will be skipped. Sorry about that!");
            }
            finally
            {
                await voiceStream.FlushAsync();
                await audioClient.StopAsync();
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