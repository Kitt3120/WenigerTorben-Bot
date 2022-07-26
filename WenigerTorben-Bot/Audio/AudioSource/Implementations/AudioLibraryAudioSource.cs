using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.Storage.Library.Audio;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Audio.AudioSource.Implementations;

public class AudioLibraryAudioSource : AudioSource
{
    public override AudioSourceType GetAudioSourceType() => AudioSourceType.Library;

    private byte[] buffer;

    public AudioLibraryAudioSource(SocketGuild guild, string request) : base(guild, request)
    {
        buffer = Array.Empty<byte>();
    }

    protected override async Task DoPrepareAsync()
    {
        LibraryStorageEntry<byte[]>? libraryStorageEntry = GetLibraryStorageEntry(guild, request);
        if (libraryStorageEntry is null)
            throw new Exception($"Unable to prepare AudioLibraryAudioSource because no LibraryStorageEntry was found for guild {guild} and request \"{request}\". Did you call IsApplicableFor() first?"); //TODO: Proper exception

        byte[]? data;
        try
        {
            data = await libraryStorageEntry.ReadAsync();
        }
        catch (Exception e)
        {
            throw new Exception("Unable to prepare AudioLibraryAudioSource because there was an error while reading the data from disk.", e); //TODO: Proper exception
        }
        if (data is null)
            throw new Exception("Unable to prepare AudioLibraryAudioSource because the desiralized data was not a byte[]."); //TODO: Proper exception

        buffer = data;
    }

    protected override Task<Stream> DoProvideAsync() => new Task<Stream>(() => new MemoryStream(buffer));

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

        LibraryStorageEntry<byte[]>? match = entries.Where(entry => entry.Title.ToLower() == request).FirstOrDefault();
        if (match is not null)
            return match;

        //TODO: More matching
        return null;
    }

    internal static bool IsApplicableFor(SocketGuild guild, string request) => GetLibraryStorageEntry(guild, request) is not null;
}