using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Serilog;
using WenigerTorbenBot.Audio.AudioSource;
using WenigerTorbenBot.Audio.Queueing;
using WenigerTorbenBot.Services.Audio;
using WenigerTorbenBot.Services.FFmpeg;
using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Storage.Library.Audio;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Storage.Library;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Modules.Text;

[Name("Audio")]
[Summary("Module to manage a guild's audio session")]
public class AudioModule : ModuleBase<SocketCommandContext>
{
    private readonly IFileService fileService;
    private readonly IFFmpegService ffmpegService;
    private readonly IAudioService audioService;
    private readonly AudioLibraryStorageService audioLibraryStorageService;

    public AudioModule(IFileService fileService, IFFmpegService ffmpegService, IAudioService audioService, AudioLibraryStorageService audioLibraryStorageService)
    {
        this.fileService = fileService;
        this.ffmpegService = ffmpegService;
        this.audioService = audioService;
        this.audioLibraryStorageService = audioLibraryStorageService;
    }

    [Command("play")]
    [Alias(new string[] { "p", "pl" })]
    [Summary("Enqueues an audio request")]
    public async Task Play(string request)
    {
        if (Context.User is not IGuildUser guildUser || Context.Channel is not ITextChannel textChannel)
        {
            await ReplyAsync("This command is only available on guilds");
            return;
        }

        IAudioSource? audioSource = AudioSource.Create(Context.Guild, request);
        if (audioSource is null)
        {
            await ReplyAsync("Sorry, I don't know how to handle that request.");
            return;
        }

        AudioRequest audioRequest = new AudioRequest(guildUser, null, textChannel, request, audioSource);

        audioRequest.AudioSource.Prepare();
        audioService.Enqueue(audioRequest);

        audioService.GetAudioSession(Context.Guild).Start();

        Log.Debug("AudioRequest {request} enqueued with AudioSourceType {audioSourceType}", request, audioSource.GetAudioSourceType());
        await ReplyAsync("Request added to queue");
    }

    [Command("pause")]
    [Alias(new string[] { "pa", "stop" })]
    [Summary("Pauses an audio session of a guild")]
    public async Task Pause()
    {
        if (Context.User is not IGuildUser || Context.Channel is not ITextChannel)
        {
            await ReplyAsync("This command is only available on guilds");
            return;
        }

        audioService.Pause(Context.Guild);
        Log.Debug("AudioSession for Guild {guild} paused by user {user}", Context.Guild.Id, Context.User.Id);
        await ReplyAsync("Audio session paused");

    }

    [Command("resume")]
    [Alias(new string[] { "r", "re" })]
    [Summary("Resumes an audio session of a guild")]
    public async Task Resume()
    {
        if (Context.User is not IGuildUser || Context.Channel is not ITextChannel)
        {
            await ReplyAsync("This command is only available on guilds");
            return;
        }

        Log.Debug("AudioSession for Guild {guild} resumed by user {user}", Context.Guild.Id, Context.User.Id);
        audioService.Resume(Context.Guild);
        await ReplyAsync("Audio session resumed");
    }

    [Command("audiolist")]
    [Alias(new string[] { "al", "alist", "audiol" })]
    [Summary("Prints a list of available audios")]
    public async Task AudioList()
    {
        if (Context.User is not IGuildUser || Context.Channel is not ITextChannel)
        {
            await ReplyAsync("This command is only available on guilds");
            return;
        }

        if (audioLibraryStorageService is null || audioLibraryStorageService.Status != Services.ServiceStatus.Started)
        {
            await ReplyAsync("The AudioLibraryStorageService was not available");
            return;
        }

        IStorage<LibraryStorageEntry<byte[]>>? library = audioLibraryStorageService.Get(Context.Guild.Id.ToString());
        if (library is null)
        {
            await ReplyAsync("No library found for this guild");
            return;
        }

        StringBuilder stringBuilder = new StringBuilder();
        foreach (LibraryStorageEntry<byte[]> libraryStorageEntry in library.GetValues())
            stringBuilder.AppendLine($"[{libraryStorageEntry.Id}] {libraryStorageEntry.Title}: {libraryStorageEntry.File}");

        string reply = stringBuilder.ToString();
        if (string.IsNullOrEmpty(reply))
            reply = "No entries found in this guild's library.";

        await ReplyAsync(reply);
    }

    [Command("audioimport")]
    [Alias(new string[] { "ai", "aimport", "audioi" })]
    [Summary("Imports audio from the web into the library of a guild.")]
    public async Task AudioImport(string url, string? title = null, string? description = null, string? tags = null, string? extras = null)
    {
        if (audioLibraryStorageService is null || audioLibraryStorageService.Status != Services.ServiceStatus.Started)
        {
            await ReplyAsync("Sorry, the Audio-Library Storage Service is currently not available. This means that I currently can't access audio data saved for this guild.");
            return;
        }

        IStorage<LibraryStorageEntry<byte[]>>? storage = audioLibraryStorageService.Get(Context.Guild.Id.ToString());
        if (storage is null || audioLibraryStorageService.Get(Context.Guild.Id.ToString()) is not ILibraryStorage<byte[]> library)
        {
            await ReplyAsync("Sorry,there is no audio library available for this guild in which I could import audio into.");
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
            title = "Unknown";

        string[]? tagsArray = null;
        Dictionary<string, string>? extrasDictionary = null;

        if (tags is not null)
            tagsArray = tags.Split(";");

        if (extras is not null)
        {
            extrasDictionary = new Dictionary<string, string>();
            foreach (string extraPair in extras.Split(";"))
            {
                string[] extraPairSplit = extraPair.Split("=");
                if (extraPairSplit.Length != 2)
                {
                    await ReplyAsync("Your syntax for one of the defined extras contains an error. Please make sure to use the correct syntax and try again.");
                    return;
                }
                extrasDictionary[extraPairSplit[0]] = extraPairSplit[1];
            }
        }

        if (!WebUtils.TryParseUri(url, out Uri? uri))
        {
            await ReplyAsync("The given string was not a valid HTTP/-S URL.");
            return;
        }

        string tempFilePath = Path.Combine(fileService.GetTempDirectory(), Guid.NewGuid().ToString());

        IUserMessage statusMessage = await ReplyAsync("Status: Downloading media");
        try
        {
            await WebUtils.DownloadToDiskAsync(uri, tempFilePath);
            await statusMessage.ModifyAsync(message => message.Content = "Status: Demuxing audio stream from media");
            byte[] data = await ffmpegService.ReadAudioAsync(tempFilePath);
            await library.Import(title, description, tagsArray, extrasDictionary, data);
            await statusMessage.ModifyAsync(message => message.Content = "Audio imported");
        }
        catch (Exception e)
        {
            Log.Error(e, "An exception occured while importing the media's audio stream from {url} into the AudioLibraryStorage of guild {guild}", url, Context.Guild.Id.ToString());
            await statusMessage.ModifyAsync(message => message.Content = "An error occured while importing your media into the audio library of your guild");
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }

    }

    //TODO: Skip and Remove command
}