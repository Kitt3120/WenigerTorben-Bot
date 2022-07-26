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
    private readonly object preparationLock;
    private Task? preparationTask;

    public static IAudioSource? Create(SocketGuild guild, string request)
    {
        //TODO: When static abstract interface members are released in C#11, implement in a better way - https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/static-abstract-interface-methods
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
        this.preparationLock = new object();
    }

    protected abstract Task DoPrepareAsync();
    protected abstract Task<Stream> DoProvideAsync();

    public void Prepare()
    {
        lock (preparationLock)
        {
            if (preparationTask is not null)
            {
                Log.Warning("Prepare() was called multiple times on AudioSource of type {audioSourceType} for request {request}. Multiple executions of Prepare() have been prevented.", GetAudioSourceType(), request);
                return;
            }

            preparationTask = DoPrepareAsync();
        }
    }

    public async Task<Stream> ProvideAsync()
    {
        if (preparationTask is null)
            throw new Exception($"Tried to access AudioSource of type {GetAudioSourceType()} for request {request} but it hasn't been prepared yet. Did you call Prepare() first?"); //TODO: Proper exception

        try
        {
            await preparationTask;
        }
        catch (Exception e)
        {
            throw new Exception($"Error while preparing AudioSource of type {GetAudioSourceType()} for request {request}.", e);
        }

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