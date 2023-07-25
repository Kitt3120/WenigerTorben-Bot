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
    public Task MetadataLoadTask { get; init; }
    public bool MetadataLoaded => MetadataLoadTask.IsCompleted;
    public IMetadata? Metadata { get; private set; }
    private readonly object contentPreparationLock;
    public Task? ContentPreparationTask { get; private set; }
    public bool ContentPrepared => ContentPreparationTask?.IsCompleted ?? false;
    protected byte[]? contentPreparationBuffer;

    public AudioSource(SocketGuild guild, string request)
    {
        this.guild = guild;
        this.request = request;
        this.MetadataLoadTask = LoadMetadataAsync();
        this.contentPreparationLock = new object();
    }

    private async Task LoadMetadataAsync() => Metadata = await DoLoadMetadataAsync();

    public async Task WhenMetadataLoadedAsync() => await MetadataLoadTask;

    private async Task PrepareContentAsync()
    {
        using MemoryStream memoryStream = new MemoryStream();
        await DoStreamAsync(memoryStream);
        contentPreparationBuffer = memoryStream.GetBuffer();
    }

    public void PrepareContent()
    {
        lock (contentPreparationLock)
        {
            if (ContentPreparationTask is not null)
            {
                Log.Warning("BeginPrepareContent() was called multiple times on AudioSource of type {audioSourceType} for request {request} in guild {guild}. Multiple executions of BeginPrepareContent() have been prevented.", AudioSourceType, request, guild.Id);
                return;
            }

            ContentPreparationTask = PrepareContentAsync();
        }
    }

    public async Task WhenContentPreparedAsync(int millisecondsDelay = 1000)
    {
        while (ContentPreparationTask is null) //No need for lock here, overhead is not worth it
            await Task.Delay(millisecondsDelay);
        await ContentPreparationTask;

    }

    public async Task StreamAsync(Stream output)
    {
        if (ContentPreparationTask is null) //No need for lock here, overhead is not worth it
            await DoStreamAsync(output); //If not cached, play directly from source. Otherwise, await finish of preparation and play cached bytes
        else
        {
            await ContentPreparationTask;
            if (contentPreparationBuffer is null)
                throw new DataException("ContentPreparationTask has finished, but contentPreparationBuffer is still null");

            await output.WriteAsync(contentPreparationBuffer);
            await output.FlushAsync();
        }
    }

    public abstract AudioSourceType AudioSourceType { get; }
    protected abstract Task<IMetadata> DoLoadMetadataAsync();
    protected abstract Task DoStreamAsync(Stream output);
}