using System;
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
    protected SocketGuild guild;
    protected string request;
    private readonly Task preparationTask;
    private Exception? exception;

    public static IAudioSource? Create(SocketGuild guild, string request)
    {
        //TODO: When static abstract interface members are released in C#11, fix this mess - https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/static-abstract-interface-methods
        if (AudioLibraryAudioSource.IsApplicableFor(guild, request))
            return new AudioLibraryAudioSource(guild, request);

        if (FileAudioSource.IsApplicableFor(request))
            return new FileAudioSource(guild, request);

        return null;
    }

    public AudioSource(SocketGuild guild, string request)
    {
        this.guild = guild;
        this.request = request;
        this.preparationTask = PrepareAsync();
    }

    protected abstract Task DoPrepareAsync();
    protected abstract Task<Stream> DoProvideAsync();

    public async Task PrepareAsync()
    {
        try
        {
            await DoPrepareAsync();
        }
        catch (Exception e)
        {
            this.exception = new Exception($"Error while preparing AudioSource of type {GetAudioSourceType()} for request {request}.", e);
            Log.Error(e, "Error while preparing AudioSource of type {audioSourceType} for request {request}.", GetAudioSourceType(), request);
        }
    }

    public async Task<Stream> ProvideAsync()
    {
        await preparationTask;
        if (this.exception is not null)
            throw this.exception;
        return await DoProvideAsync();
    }

    public async Task WriteToAsync(Stream stream)
    {
        using Stream source = await ProvideAsync();
        source.CopyTo(stream);
        await source.CopyToAsync(stream);
    }

    public abstract AudioSourceType GetAudioSourceType();
}