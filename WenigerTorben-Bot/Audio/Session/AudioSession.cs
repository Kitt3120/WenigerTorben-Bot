using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Serilog;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Session;

public class AudioSession : IAudioSession
{
    public IGuild Guild { get; init; }
    public AudioApplication AudioApplication { get; set; }
    public int Bitrate { get; set; }
    public int BufferMillis { get; set; }
    public int StepSize => Convert.ToInt32(((Bitrate / 8.0D) / (BufferMillis / 1000.0D)) / 2);
    public bool Paused => !pauseEvent.WaitOne(0);
    public bool HasReachedEnd
    {
        get
        {
            lock (reachedEndLock)
                return reachedEnd;
        }
        set
        {
            lock (reachedEndLock)
                reachedEnd = value;
        }
    }
    public IAudioRequestQueue AudioRequestQueue { get; init; }
    public int Position
    {
        get
        {
            lock (positionLock)
                return position;
        }
        set
        {
            if (AudioRequestQueue.IsEmpty)
            {
                HasReachedEnd = true;
                value = 0;
            }
            else
            {
                HasReachedEnd = false;

                if (value < 0)
                    value = 0;
                else if (value >= AudioRequestQueue.Count)
                    value = AudioRequestQueue.Count - 1;
            }

            lock (positionLock)
                position = value;
        }
    }

    private readonly ManualResetEvent pauseEvent;
    private readonly object reachedEndLock;
    private bool reachedEnd;
    private readonly object positionLock;
    private int position;
    private readonly object bufferLock;
    private readonly Thread queueThread;

    public AudioSession(IGuild guild, AudioApplication audioApplication = AudioApplication.Music, int bitrate = 96000, int bufferMillis = 1000)
    {
        this.Guild = guild;

        this.AudioApplication = audioApplication;
        this.Bitrate = bitrate;
        this.BufferMillis = bufferMillis;
        this.AudioRequestQueue = new AudioRequestQueue();

        this.pauseEvent = new ManualResetEvent(false);
        this.reachedEndLock = new object();
        this.reachedEnd = true;
        this.positionLock = new object();
        this.position = 0;
        this.bufferLock = new object();
        this.queueThread = new Thread(HandleQueue);

        AudioRequestQueue.OnEnqueue += OnEnqueueRequest;
        AudioRequestQueue.OnDequeue += OnDequeueRequest;


        this.queueThread.Start();
    }

    private void OnEnqueueRequest(object? sender, EventArgs e)
    {
        if (HasReachedEnd)
        {
            if (AudioRequestQueue.Count > 1)
                Position++; //This also sets HasReachedEnd to false
            else
                HasReachedEnd = false;
            Resume();
        }
    }

    private void OnDequeueRequest(object? sender, EventArgs e)
    {
        if (Position >= AudioRequestQueue.Count)
            Position = AudioRequestQueue.Count - 1;
    }

    public void Pause() => pauseEvent.Reset();

    public void Resume()
    {
        HasReachedEnd = false;
        pauseEvent.Set();
    }

    public void Skip()
    {

    }

    private void Next()
    {
        if (Position == AudioRequestQueue.Count - 1)
            HasReachedEnd = true;
        else
            Position++;
    }

    private async void HandleQueue()
    {
        while (true)
        {
            pauseEvent.WaitOne();

            if (AudioRequestQueue.IsEmpty || HasReachedEnd)
            {
                Pause();
                continue;
            }

            IAudioRequest? audioRequest = AudioRequestQueue.GetAtPosition(Position);
            if (audioRequest is null)
            {
                Pause();
                Log.Error("AudioRequest at position {position} in queue of AudioSession for guild {guildName} ({guildId}) was null. AudioSession has been paused.", Position, Guild.Name, Guild.Id);
                continue;
            }

            IVoiceChannel? targetChannel = audioRequest.TargetChannel;
            if (targetChannel is null)
            {
                await audioRequest.OriginChannel.SendMessageAsync($"Sorry {audioRequest.Requestor.Mention}, I was not able to determine the voice channel you're in. Your request will be skipped.");
                Next();
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
                voiceStream = audioClient.CreatePCMStream(AudioApplication, Bitrate, BufferMillis);

                //Read from stream while it is still being writte to
                do
                {
                    //Calculate bytes to read based on positionLimit, buffer and maxStepSize
                    int availableBytesAmount = Math.Min(positionLimit - position, buffer.Length - position);
                    if (availableBytesAmount > StepSize)
                        availableBytesAmount = StepSize;

                    if (availableBytesAmount > 0)
                    {
                        //No locking of bufferLock needed, as we're just reading. Worst case we're reading from the old buffer for this iteration, which would be no problem.
                        Memory<byte> data = buffer.AsMemory(position, availableBytesAmount);
                        await voiceStream.WriteAsync(data);
                        position += availableBytesAmount;
                    }
                    else
                        await Task.Delay(BufferMillis / 2); //Let streams buffer a bit in case audio output is faster than input

                    pauseEvent.WaitOne();

                    if (!streamTask.IsCompleted) //If new input is still being streamed, update reference in case a new buffer has been allocated by memoryStream
                        lock (bufferLock)
                            buffer = memoryStream.GetBuffer();

                } while (!streamTask.IsCompleted || (position < buffer.Length && position < positionLimit));

                await streamTask; //Await so exception gets thrown if there was any while executing the task
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while streaming audio of request {audioRequest} to channel {channelName} ({channelId}) in guild {guildName} ({guildId}). Skipping entry.", audioRequest.Request, audioRequest.TargetChannel.Name, audioRequest.TargetChannel.Id, Guild.Name, Guild.Id);
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

                Next();
            }
        }
    }

}