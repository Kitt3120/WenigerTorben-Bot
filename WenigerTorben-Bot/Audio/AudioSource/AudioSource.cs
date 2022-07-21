using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using WenigerTorbenBot.Audio.AudioSource.Implementations;
using WenigerTorbenBot.Audio.Queueing;

namespace WenigerTorbenBot.Audio.AudioSource;

public abstract class AudioSource : IAudioSource
{
    protected string request;
    private Task preparationTask;
    private Exception? exception;

    public static IAudioSource? Create(string request)
    {
        if (FileAudioSource.IsApplicableFor(request))
            return new FileAudioSource(request);

        return null;
    }

    public AudioSource(string request)
    {
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
        if (exception is not null)
            throw exception;
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