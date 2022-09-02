using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Serilog;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Session;

public class AudioSession : IAudioSession
{
    public IGuild Guild { get; private set; }

    private AudioApplication audioApplication;
    private int bitrate;
    private int bufferMillis;
    private int stepSize;

    private readonly object queueLock;
    private readonly List<AudioRequest> queue;
    private readonly ManualResetEvent pauseEvent;
    private readonly object autoPauseLock;
    private bool autoPaused;
    private readonly object bufferLock;
    private readonly Thread queueThread;

    public AudioSession(IGuild guild, AudioApplication audioApplication = AudioApplication.Music, int bitrate = 96000, int bufferMillis = 1000)
    {
        this.Guild = guild;

        this.audioApplication = audioApplication;
        this.bitrate = bitrate;
        this.bufferMillis = bufferMillis;
        this.stepSize = Convert.ToInt32(((bitrate / 8.0D) / (bufferMillis / 1000.0D)) / 2);

        this.queueLock = new object();
        this.queue = new List<AudioRequest>();
        this.pauseEvent = new ManualResetEvent(false);
        this.autoPauseLock = new object();
        this.autoPaused = true;
        this.bufferLock = new object();
        this.queueThread = new Thread(HandleQueue);
        this.queueThread.Start();
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

    public void SetAudioApplication(AudioApplication audioApplication) => this.audioApplication = audioApplication;

    public void SetBitrate(int bitrate)
    {
        this.bitrate = bitrate;
        this.stepSize = Convert.ToInt32(((bitrate / 8.0D) / (bufferMillis / 1000.0D)) / 2);
    }

    public void SetBufferMillis(int bufferMillis)
    {
        this.bufferMillis = bufferMillis;
        this.stepSize = Convert.ToInt32(((bitrate / 8.0D) / (bufferMillis / 1000.0D)) / 2);
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

            using MemoryStream memoryStream = new MemoryStream();
            byte[] buffer = memoryStream.GetBuffer();

            int position = 0;
            int positionLimit = int.MaxValue;

            Task streamTask = Task.Run(async () =>
            {
                await audioRequest.AudioSource.StreamAsync(memoryStream);

                /*
                Detect empty remaining bytes in memoryStream buffer.
                This happens because of the dynamic resizing of the underlying buffer of a MemoryStream.
                If we would not force-end the output through positionLimit here, the bot would remain in audio channels and send 0-byte audio for a while after playing any media.
                To make things simpler: We're "trimming" the remaining 0-bytes from the end of memoryStream. But we're not actually trimming the buffer array, we're just setting a limit for the playback.
                */
                lock (bufferLock)
                {
                    buffer = memoryStream.GetBuffer();
                    int lastDataPosition = position;
                    for (int i = lastDataPosition; i < buffer.Length; i++)
                        if (buffer[i] != 0)
                            lastDataPosition = i;
                    positionLimit = lastDataPosition + 1; //Adding +1 offset so it behaves as a limit, same as buffer.Length, that should not be reached or exceeded 
                }
            });

            IAudioClient? audioClient = null;
            AudioOutStream? voiceStream = null;
            try
            {
                audioClient = await targetChannel.ConnectAsync();
                voiceStream = audioClient.CreatePCMStream(audioApplication, bitrate, bufferMillis);

                //Read from stream while it is still being writte to
                do
                {
                    //Calculate bytes to read based on positionLimit, buffer and maxStepSize
                    int availableBytesAmount = Math.Min(positionLimit - position, buffer.Length - position);
                    if (availableBytesAmount > stepSize)
                        availableBytesAmount = stepSize;

                    if (availableBytesAmount > 0)
                    {
                        //No locking of bufferLock needed, as we're just reading. Worst case we're reading from the old buffer for this iteration, which would be no problem.
                        Memory<byte> data = buffer.AsMemory(position, availableBytesAmount);
                        await voiceStream.WriteAsync(data);
                        position += availableBytesAmount;
                    }
                    else
                        await Task.Delay(bufferMillis / 2); //Let streams buffer a bit in case audio output is faster than input

                    pauseEvent.WaitOne();

                    if (!streamTask.IsCompleted) //If new input is still being streamed, update reference in case a new buffer has been allocated by memoryStream
                        lock (bufferLock)
                            buffer = memoryStream.GetBuffer();

                } while (!streamTask.IsCompleted || (position < buffer.Length && position < positionLimit));

                await streamTask; //Await so exception gets thrown if there was any while executing the task
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