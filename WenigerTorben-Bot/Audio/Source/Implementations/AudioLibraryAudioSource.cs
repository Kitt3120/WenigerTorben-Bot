using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Metadata;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.Storage.Library.Audio;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Audio.Source.Implementations;

public class AudioLibraryAudioSource : AudioSource
{
    public static bool IsApplicableFor(SocketGuild guild, string request) => GetLibraryStorageEntry(guild, request) is not null;

    public override AudioSourceType AudioSourceType => AudioSourceType.AudioLibrary;

    public AudioLibraryAudioSource(SocketGuild guild, string request) : base(guild, request)
    { }

    protected override async Task DoStreamAsync(Stream output)
    {
        LibraryStorageEntry<byte[]>? libraryStorageEntry = GetLibraryStorageEntry(guild, request);
        if (libraryStorageEntry is null)
            throw new Exception($"No entry was found for request \"{request}\" in AudioLibraryStorage of guild {guild}"); //TODO: Proper exception

        byte[]? data;
        try
        {
            data = await libraryStorageEntry.ReadAsync();
        }
        catch (Exception e)
        {
            throw new Exception("There was an error while reading the data from disk.", e); //TODO: Proper exception
        }
        if (data is null)
            throw new Exception("The desiralized data was null."); //TODO: Proper exception
        else if (data.Length == 0)
            throw new ArgumentException("The media at the given path contained no audio to be extracted", nameof(request));

        await output.WriteAsync(data);
        await output.FlushAsync();
    }

    protected override Task<IMetadata> DoLoadMetadataAsync()
    {
        LibraryStorageEntry<byte[]>? libraryStorageEntry = GetLibraryStorageEntry(guild, request);
        if (libraryStorageEntry is null)
            throw new Exception($"No LibraryStorageEntry was found for guild {guild} and request \"{request}\". Did you call IsApplicableFor() first?"); //TODO: Proper exception

        return Task.FromResult(libraryStorageEntry.Metadata as IMetadata);
    }

    private static LibraryStorageEntry<byte[]>? GetLibraryStorageEntry(SocketGuild guild, string request)
    {
        //TODO: Logging
        AudioLibraryStorageService? audioLibraryStorageService = ServiceRegistry.Get<AudioLibraryStorageService>();
        if (audioLibraryStorageService is null)
            return null;

        IStorage<LibraryStorageEntry<byte[]>>? guildStorage = audioLibraryStorageService.Get(guild.Id.ToString());
        if (guildStorage is null)
            return null;

        LibraryStorageEntry<byte[]>[] entries = guildStorage.GetValues();
        request = request.ToLower();

        LibraryStorageEntry<byte[]>? match = entries.Where(entry => entry.Metadata.Title?.ToLower() == request).FirstOrDefault();
        if (match is not null)
            return match;

        //TODO: More matching
        return null;
    }
}