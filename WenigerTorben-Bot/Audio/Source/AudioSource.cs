using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using WenigerTorbenBot.Audio.Queueing;
using WenigerTorbenBot.Audio.Source.Implementations;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Services.Storage.Library.Audio;

namespace WenigerTorbenBot.Audio.Source;

public abstract class AudioSource : IAudioSource
{
    public static IAudioSource? Create(SocketGuild guild, string request)
    {
        //TODO: When static abstract interface members are released in C#11, implement in a better way - https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/static-abstract-interface-methods
        if (FileAudioSource.IsApplicableFor(request))
            return new FileAudioSource(guild, request);
        else if (YouTubeAudioSource.IsApplicableFor(request))
            return new YouTubeAudioSource(guild, request);
        else if (AudioLibraryAudioSource.IsApplicableFor(guild, request))
            return new AudioLibraryAudioSource(guild, request);
        else if (WebAudioSource.IsApplicableFor(request))
            return new WebAudioSource(guild, request);
        else
            return null;
    }

    protected readonly SocketGuild guild;
    protected readonly string request;
    private readonly Task metadataLoadTask;
    private IMetadata? audioSourceMetadata;
    private readonly object contentPreparationLock;
    protected byte[]? contentPreparationBuffer;
    private Task? contentPreparationTask;

    public AudioSource(SocketGuild guild, string request)
    {
        this.guild = guild;
        this.request = request;
        this.metadataLoadTask = LoadMetadataAsync();
        this.contentPreparationLock = new object();
    }

    private async Task LoadMetadataAsync() => audioSourceMetadata = await DoLoadMetadataAsync();

    public Task WhenMetadataLoaded() => metadataLoadTask;

    public IMetadata GetAudioSourceMetadata()
    {
        if (audioSourceMetadata is null)
            throw new NullReferenceException("\"audioSourceMetadata\" was null. Did you await WhenMetadataLoaded() first?");
        return audioSourceMetadata;
    }

    private async Task PrepareContentAsync()
    {
        using MemoryStream memoryStream = new MemoryStream();
        await DoStreamAsync(memoryStream);
        contentPreparationBuffer = memoryStream.GetBuffer();
    }

    public void BeginPrepareContent()
    {
        lock (contentPreparationLock)
        {
            if (contentPreparationTask is not null)
            {
                Log.Warning("BeginPrepareContent() was called multiple times on AudioSource of type {audioSourceType} for request {request} in guild {guild}. Multiple executions of BeginPrepareContent() have been prevented.", GetAudioSourceType(), request, guild.Id);
                return;
            }

            contentPreparationTask = PrepareContentAsync();
        }
    }

    public async Task WhenContentPrepared(bool autoStartContentPreparation = false)
    {
        bool isContentPreparationTaskNull;
        lock (contentPreparationLock)
            isContentPreparationTaskNull = contentPreparationTask is null;

        if (isContentPreparationTaskNull)
        {
            if (autoStartContentPreparation)
                BeginPrepareContent();
            else
                while (contentPreparationTask is null) //No need for lock here, overhead is not worth it
                    await Task.Delay(1000);
        }

        await contentPreparationTask;
    }

    public async Task StreamAsync(Stream output)
    {
        bool isContentPreparationTaskNull;
        lock (contentPreparationLock)
            isContentPreparationTaskNull = contentPreparationTask is null;

        if (isContentPreparationTaskNull) //If not cached, play directly from source. Otherwise, await finish of preparation and play cached bytes
            await DoStreamAsync(output);
        else
        {
            await contentPreparationTask;
            using MemoryStream memoryStream = new MemoryStream(contentPreparationBuffer, false);
            await memoryStream.CopyToAsync(output);
        }
    }

    protected abstract Task<IMetadata> DoLoadMetadataAsync();
    protected abstract Task DoStreamAsync(Stream output);

    public abstract AudioSourceType GetAudioSourceType();
}