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
using WenigerTorbenBot.Audio.Player;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Session;

//TODO: Implement caching through IAudioSource.PrepareAsync()
public class AudioSession : IAudioSession
{
    public IGuild Guild { get; init; }
    public IAudioPlayer AudioPlayer { get; init; }
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
            SetPosition(value);
        }
    }

    public bool HasReachedEnd
    {
        get
        {
            lock (reachedEndLock)
                return reachedEnd;
        }
        private set
        {
            lock (reachedEndLock)
                reachedEnd = value;
        }
    }

    public EventHandler<PositionChangeEventArgs>? OnPositionChange { get; set; }

    private readonly ManualResetEvent softPauseResetEvent;
    private readonly object reachedEndLock;
    private bool reachedEnd;
    private readonly object positionLock;
    private int position;
    private bool noPositionIncrement;
    private readonly ManualResetEvent playingResetEvent;
    private readonly Thread queueThread;

    public AudioSession(IGuild guild, AudioApplication audioApplication = AudioApplication.Music, int? bitrate = null, int bufferMillis = 1000)
    {
        this.Guild = guild;
        this.AudioPlayer = new AudioPlayer(guild, audioApplication, bitrate, bufferMillis);
        this.AudioRequestQueue = new AudioRequestQueue();

        this.softPauseResetEvent = new ManualResetEvent(true);
        this.reachedEndLock = new object();
        this.reachedEnd = true;
        this.positionLock = new object();
        this.position = 0;
        this.noPositionIncrement = false;
        this.playingResetEvent = new ManualResetEvent(false);
        this.queueThread = new Thread(HandleQueue);

        AudioRequestQueue.OnEnqueue += OnEnqueueRequest;
        AudioRequestQueue.OnDequeue += OnDequeueRequest;
        AudioRequestQueue.OnSwap += OnSwapRequests;
        AudioPlayer.OnFinish += OnFinish;


        this.queueThread.Start();
    }

    private void OnEnqueueRequest(object? sender, EnqueueEventArgs enqueueEventArgs)
    {
        if (AudioRequestQueue.Count == 1)
        {
            Position = 0;
            softPauseResetEvent.Set();
        }
        else
        {
            bool hadReachedEnd = HasReachedEnd;

            if (enqueueEventArgs.Position <= Position)
            {
                Position++;
                HasReachedEnd = hadReachedEnd;
            }
            else if (HasReachedEnd)
            {
                Position++;
                softPauseResetEvent.Set();
            }
        }
    }

    private void OnDequeueRequest(object? sender, DequeueEventArgs dequeueEventArgs)
    {
        if (AudioRequestQueue.Count == 0)
        {
            Position = 0;
            HasReachedEnd = true;
            AudioPlayer.Cancel();
        }
        else
        {
            if (dequeueEventArgs.Position == Position)
            {
                if (dequeueEventArgs.Position == AudioRequestQueue.Count)
                {
                    Position--;
                    HasReachedEnd = true;
                }

                if (AudioPlayer.CurrentPlayTask is not null && !AudioPlayer.CurrentPlayTask.IsCompleted)
                {
                    noPositionIncrement = true;
                    AudioPlayer.Cancel();
                }
            }
            else if (dequeueEventArgs.Position < Position)
            {
                bool hadReachedEnd = HasReachedEnd;
                Position--;
                HasReachedEnd = hadReachedEnd;
            }
        }
    }

    private void OnSwapRequests(object? sender, QueueSwapEventArgs queueSwapEventArgs)
    {
        if (queueSwapEventArgs.Position1 == Position)
            Position = queueSwapEventArgs.Position2;
        else if (queueSwapEventArgs.Position2 == Position)
            Position = queueSwapEventArgs.Position1;
    }

    private void OnFinish(object? sender, FinishedEventArgs finishedEventArgs)
    {
        Next();
        playingResetEvent.Set();
    }

    public void Pause()
    {
        AudioPlayer.Paused = true;
    }

    public void Resume()
    {
        if (HasReachedEnd)
            Position = 0;

        AudioPlayer.Paused = false;
        softPauseResetEvent.Set();
    }

    public void Skip()
    {
        AudioPlayer.Cancel();
        AudioPlayer.Paused = false;
        softPauseResetEvent.Set();
    }

    public void Previous()
    {
        if (Position > 0)
            Position--;

        HasReachedEnd = false;
        noPositionIncrement = true;
        AudioPlayer.Cancel();
        AudioPlayer.Paused = false;
        softPauseResetEvent.Set();
    }

    private void Next()
    {
        if (noPositionIncrement)
        {
            noPositionIncrement = false;
            return;
        }

        if (Position == AudioRequestQueue.Count - 1)
            HasReachedEnd = true;
        else
            Position++;
    }

    private void SetPosition(int value)
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

        int previousPosition = position;

        lock (positionLock)
            position = value;

        OnPositionChange?.Invoke(this, new PositionChangeEventArgs(previousPosition, value));
    }

    private void HandleQueue()
    {
        while (true)
        {
            softPauseResetEvent.WaitOne();

            if (AudioRequestQueue.IsEmpty || HasReachedEnd)
            {
                softPauseResetEvent.Reset();
                continue;
            }

            IAudioRequest? audioRequest = AudioRequestQueue.GetAtPosition(Position);
            if (audioRequest is null)
            {
                Pause();
                Log.Error("AudioRequest at position {position} in queue of AudioSession for guild {guildName} ({guildId}) was null. AudioSession has been paused.", Position, Guild.Name, Guild.Id);
                continue;
            }

            playingResetEvent.Reset();
            AudioPlayer.Play(audioRequest);
            playingResetEvent.WaitOne();
        }
    }

}