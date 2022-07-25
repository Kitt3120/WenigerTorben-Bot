using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
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
    { }

    protected override Task DoPrepareAsync()
    {
        //TODO: Implement
    }

    protected override Task<Stream> DoProvideAsync()
    {
        throw new System.NotImplementedException();
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

        LibraryStorageEntry<byte[]>? match = entries.Where(entry => entry.Title.ToLower() == request).FirstOrDefault();
        if (match is not null)
            return match;

        //TODO: More matching
        return null;
    }

    internal static bool IsApplicableFor(SocketGuild guild, string request) => GetLibraryStorageEntry(guild, request) is not null;
}