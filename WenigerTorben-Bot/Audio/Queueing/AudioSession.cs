using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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

            /* try
            {
                await audioRequest.AudioSource.WhenPrepared();
            }
            catch (Exception e)
            {
                if (e is not ArgumentException && e is not HttpRequestException)
                    Log.Error(e, "An error occured while preparing an AudioSource.\nRequest: {request}.", audioRequest.Request);

                await audioRequest.OriginChannel.SendMessageAsync($"Sorry {audioRequest.Requestor.Mention}, there was an error while preparing your requested audio: {e.Message}\nYour request will be skipped. Sorry about that!");
                Dequeue(audioRequest);
                continue;
            } */

            using MemoryStream memoryStream = new MemoryStream();
            Task streamTask = audioRequest.AudioSource.StreamAsync(memoryStream);

            int bitrate = 96000;
            int bufferMillis = 1000;

            bool shouldUpdateBufferReference = true;
            byte[] buffer = memoryStream.GetBuffer();
            int position = 0;
            int maxStepSize = Convert.ToInt32(((bitrate / 8.0D) / (bufferMillis / 1000.0D)) / 2); //2 * 1024;
            IAudioClient? audioClient = null;
            AudioOutStream? voiceStream = null;
            try
            {
                audioClient = await targetChannel.ConnectAsync();
                voiceStream = audioClient.CreatePCMStream(AudioApplication.Music, bitrate, bufferMillis);

                //Read from stream while it is still being writte to
                do
                {
                    if (shouldUpdateBufferReference)
                    {
                        buffer = memoryStream.GetBuffer();
                        shouldUpdateBufferReference = !streamTask.IsCompleted;
                    }

                    int availableBytesAmount = buffer.Length - position;
                    if (availableBytesAmount > maxStepSize)
                        availableBytesAmount = maxStepSize;

                    if (availableBytesAmount > 0)
                    {
                        //Detect empty remaining bytes. This happens because of the dynamic resizing of the underlying buffer of a MemoryStream.
                        //If we would not force-end here, the bot would remain in audio channels and send 0-byte audio for a while after playing any media.
                        if (streamTask.IsCompleted && buffer[position] == 0)
                        {
                            bool foundRemainingData = false;
                            for (int i = position; i < buffer.Length; i++)
                            {
                                if (buffer[i] != 0)
                                {
                                    foundRemainingData = true;
                                    break;
                                }
                            }

                            if (!foundRemainingData)
                                break;
                        }
                        Memory<byte> data = buffer.AsMemory(position, availableBytesAmount);
                        await voiceStream.WriteAsync(data);
                        position += availableBytesAmount;
                    }
                    else
                        await Task.Delay(bufferMillis / 2); //Let streams buffer a bit in case audio output is faster than input
                    pauseEvent.WaitOne();

                    if (shouldUpdateBufferReference)
                    {
                        buffer = memoryStream.GetBuffer();
                        shouldUpdateBufferReference = !streamTask.IsCompleted;
                    }
                } while (!streamTask.IsCompleted || position < buffer.Length);
                await streamTask;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while streaming audio of request {audioRequest} to channel {channel} in guild {guild}. Skipping entry.", audioRequest.Request, audioRequest.GetTargetChannel(), audioRequest.GetTargetChannel().Guild.Id);
                await audioRequest.OriginChannel.SendMessageAsync($"Sorry {audioRequest.Requestor.Mention}, there was an error while playing your requested audio: {e.Message}\nYour request will be skipped. Sorry about that!");
            }
            finally
            {
                if (voiceStream is not null)
                    await voiceStream.FlushAsync();
                if (audioClient is not null)
                {
                    await audioClient.StopAsync();
                    audioClient.Dispose();
                }
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