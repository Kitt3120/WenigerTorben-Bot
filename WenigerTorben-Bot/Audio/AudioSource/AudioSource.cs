using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using WenigerTorbenBot.Audio.AudioSource.Implementations;
using WenigerTorbenBot.Audio.Queueing;
using WenigerTorbenBot.Services.Storage.Library.Audio;

namespace WenigerTorbenBot.Audio.AudioSource;

public abstract class AudioSource : IAudioSource
{
    protected readonly string request;
    protected byte[] buffer;
    private readonly object preparationLock;
    private Task? preparationTask;

    public static IAudioSource? Create(SocketGuild guild, string request)
    {
        //TODO: When static abstract interface members are released in C#11, implement in a better way - https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/static-abstract-interface-methods
        if (FileAudioSource.IsApplicableFor(request))
            return new FileAudioSource(request);
        else if (YouTubeAudioSource.IsApplicableFor(request))
            return new YouTubeAudioSource(request);
        else if (WebAudioSource.IsApplicableFor(request))
            return new WebAudioSource(request);
        else if (AudioLibraryAudioSource.IsApplicableFor(guild, request))
            return new AudioLibraryAudioSource(guild, request);
        else
            return null;
    }

    public AudioSource(string request)
    {
        this.request = request;
        this.buffer = Array.Empty<byte>();
        this.preparationLock = new object();
    }

    protected abstract Task DoPrepareAsync();

    public void BeginPrepare()
    {
        lock (preparationLock)
        {
            if (preparationTask is not null)
            {
                Log.Warning("BeginPrepare() was called multiple times on AudioSource of type {audioSourceType} for request {request}. Multiple executions of BeginPrepare() have been prevented.", GetAudioSourceType(), request);
                return;
            }

            preparationTask = DoPrepareAsync();
        }
    }

    public async Task WhenPrepared()
    {
        bool isPreparationTaskNull;
        lock (preparationLock)
            isPreparationTaskNull = preparationTask is null;

        if (isPreparationTaskNull)
        {
            Log.Debug("WhenPrepared() was called before BeginPrepare() on AudioSource of type {audioSourceType} for request {request}. Calling BeginPrepare() first.", GetAudioSourceType(), request);
            BeginPrepare();
        }

        await preparationTask;
    }

    public IReadOnlyCollection<byte> GetData() => Array.AsReadOnly(buffer);

    public MemoryStream CreateStream() => new MemoryStream(buffer, false);

    public async Task WriteAsync(Stream stream) => await stream.WriteAsync(buffer);

    public abstract AudioSourceType GetAudioSourceType();
}