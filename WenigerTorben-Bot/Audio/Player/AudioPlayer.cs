using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Serilog;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.Player;

public class AudioPlayer : IAudioPlayer
{
    public IGuild Guild { get; init; }
    public IVoiceChannel? VoiceChannel { get; private set; }
    public AudioApplication AudioApplication { get; set; }
    public bool AutoBitrate { get; set; }
    public int Bitrate { get; set; }
    public int BufferMillis { get; set; }
    public int StepSize => Convert.ToInt32(((Bitrate / 8.0D) / (BufferMillis / 1000.0D)) / 2);
    public bool Paused
    {
        get => !pauseResetEvent.WaitOne(0);
        set
        {
            if (value)
                pauseResetEvent.Reset();
            else
                pauseResetEvent.Set();
        }
    }
    public Task? CurrentPlayTask { get; private set; }

    public EventHandler<FinishedEventArgs>? OnFinish { get; set; }

    private readonly object playHandleLock;
    private CancellationTokenSource? playCancelTokenSource;
    private IAudioClient? audioClient;
    private AudioOutStream? audioOutStream;
    private readonly ManualResetEvent pauseResetEvent;

    public AudioPlayer(IGuild guild, AudioApplication audioApplication = AudioApplication.Music, int? bitrate = null, int bufferMillis = 1000)
    {
        Guild = guild;
        AudioApplication = audioApplication;
        AutoBitrate = bitrate is null;
        Bitrate = bitrate ?? 0;
        BufferMillis = bufferMillis;
        playHandleLock = new object();
        pauseResetEvent = new ManualResetEvent(true);
    }

    public void Play(IAudioRequest audioRequest)
    {
        lock (playHandleLock)
        {
            if (CurrentPlayTask is not null && !CurrentPlayTask.IsCompleted)
            {
                Cancel();
                CurrentPlayTask.Wait();
            }

            playCancelTokenSource = new CancellationTokenSource();
            CurrentPlayTask = Task.Run(async () =>
            {
                try
                {
                    await PlayAudio(audioRequest);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error while streaming audio of request {audioRequest} to channel {channelName} ({channelId}) in guild {guildName} ({guildId}). Skipping entry.", audioRequest.Request, audioRequest.VoiceChannel.Name, audioRequest.VoiceChannel.Id, Guild.Name, Guild.Id);
                    await audioRequest.OriginChannel.SendMessageAsync($"Sorry {audioRequest.Requestor.Mention}, there was an error while playing your requested audio: {e.Message}\nYour request will be skipped. Sorry about that!");
                }
                finally
                {
                    InvokeOnFinish(audioRequest);
                    playCancelTokenSource.Dispose();
                    playCancelTokenSource = null;
                    CurrentPlayTask = null;
                }
            }, playCancelTokenSource.Token);
        }
    }

    public void Cancel()
    {
        lock (playHandleLock)
            if (playCancelTokenSource is not null
            && !playCancelTokenSource.IsCancellationRequested
            && CurrentPlayTask is not null
            && !CurrentPlayTask.IsCompleted)
                playCancelTokenSource.Cancel();
    }

    private async Task PlayAudio(IAudioRequest audioRequest)
    {
        if (playCancelTokenSource is null)
            throw new InvalidOperationException("PlayAudio() was called without a valid CancellationTokenSource");
        else if (playCancelTokenSource.IsCancellationRequested)
            throw new InvalidOperationException("PlayAudio() was called with a cancelled CancellationTokenSource");

        object bufferLock = new object();
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
                for (int i = lastDataPosition; i < buffer.Length || playCancelTokenSource.IsCancellationRequested; i++)
                    if (buffer[i] != 0)
                        lastDataPosition = i;
                positionLimit = lastDataPosition + 1; //Adding +1 offset so it behaves as a limit, same as buffer.Length, that should not be reached or exceeded 
            }
        }, playCancelTokenSource.Token);

        IVoiceChannel targetChannel = audioRequest.VoiceChannel;

        if (audioOutStream is not null)
            await audioOutStream.DisposeAsync();

        if (VoiceChannel is not null && targetChannel != VoiceChannel && audioClient is not null)
        {
            await audioClient.StopAsync();
            audioClient.Dispose();
            audioClient = null;
        }

        if (audioClient is null)
            audioClient = await targetChannel.ConnectAsync();

        if (AutoBitrate)
            Bitrate = targetChannel.Bitrate;

        audioOutStream = audioClient.CreatePCMStream(AudioApplication, Bitrate, BufferMillis);
        VoiceChannel = targetChannel;

        //Read from stream while it is still being writte to
        do
        {
            pauseResetEvent.WaitOne();
            if (playCancelTokenSource.IsCancellationRequested)
                break;

            //Calculate bytes to read based on positionLimit, buffer and maxStepSize
            int availableBytesAmount = Math.Min(positionLimit - position, buffer.Length - position);
            if (availableBytesAmount > StepSize)
                availableBytesAmount = StepSize;

            if (!streamTask.IsCompleted && availableBytesAmount < StepSize)
            {
                await Task.Delay(BufferMillis); //Let streams buffer a bit in case audio output is faster than input
            }
            else
            {
                //No locking of bufferLock needed, as we're just reading. Worst case we're reading from the old buffer for this iteration, which would be no problem.
                Memory<byte> data = buffer.AsMemory(position, availableBytesAmount);
                await audioOutStream.WriteAsync(data);
                position += availableBytesAmount;
            }

            if (!streamTask.IsCompleted) //If new input is still being streamed, update reference in case a new buffer has been allocated by the MemoryStream
                lock (bufferLock)
                    buffer = memoryStream.GetBuffer();

        } while (!streamTask.IsCompleted || (position < buffer.Length && position < positionLimit));

        //Sometimes, FlushAsync gets stuck, so we're using a timeout here
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Task flushTask = audioOutStream.FlushAsync(cancellationTokenSource.Token);
        Task timeoutTask = Task.Delay(BufferMillis * 2, cancellationTokenSource.Token);
        Task completedTask = await Task.WhenAny(flushTask, timeoutTask);
        if (completedTask == timeoutTask)
        {
            cancellationTokenSource.Cancel();
            Log.Warning("FlushAsync of AudioOutStream for request {audioRequest} in channel {channelName} ({channelId}) in guild {guildName} ({guildId}) timed out. Skipping FlushAsync call.", audioRequest.Request, audioRequest.VoiceChannel.Name, audioRequest.VoiceChannel.Id, Guild.Name, Guild.Id);
        }

        await streamTask; //Await so exception gets thrown if there was any while executing the task

    }

    private void InvokeOnFinish(IAudioRequest audioRequest)
    {
        try
        {
            OnFinish?.Invoke(this, new FinishedEventArgs());
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while invoking OnFinish event handler in AudioPlayer for VoiceChannel {voiceChannelName} ({voiceChannelId}) of guild {guildName} ({guildId}) for request {request}", audioRequest.VoiceChannel.Name, audioRequest.VoiceChannel.Id, Guild.Name, Guild.Id, audioRequest.Request);
        }
    }
}