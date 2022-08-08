using System;
using System.Collections.Generic;
using System.IO;
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

        audioRequest.AudioSource.BeginPrepare();
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

    [Command("audio")]
    [Alias("a")]
    [Summary("Manages the audio library of your guild")]
    public async Task Audio(string? subcommand = null, string? url = null, string? title = null, string? description = null, string? tags = null, string? extras = null)
    {
        //TODO: Make modular subcommand system, this sucks
        if (subcommand is null)
        {
            List<EmbedFieldBuilder> embedFields = new List<EmbedFieldBuilder>();
            embedFields.Add(new EmbedFieldBuilder().WithName("audio").WithValue("Prints this help"));
            embedFields.Add(new EmbedFieldBuilder().WithName("audio list/ls").WithValue("Prints a list of available audios for this guild"));
            embedFields.Add(new EmbedFieldBuilder().WithName("audio import/add <url> <title> [description] [tag1;tag2] [extra1=value1;extra2=value2]").WithValue("Imports audio from the web into the library of a guild"));

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Audio commands");
            embedBuilder.WithFields(embedFields);
            embedBuilder.WithColor(Color.Red);

            await ReplyAsync(embed: embedBuilder.Build());
            return;
        }

        if (Context.User is not IGuildUser || Context.Channel is not ITextChannel)
        {
            await ReplyAsync("This command is only available on guilds");
            return;
        }

        if (audioLibraryStorageService is null || audioLibraryStorageService.Status != Services.ServiceStatus.Started)
        {
            await ReplyAsync("Sorry, the storage service is currently not available. This means that I can't access audio data saved for this guild.");
            return;
        }

        IStorage<LibraryStorageEntry<byte[]>>? storage = audioLibraryStorageService.Get(Context.Guild.Id.ToString());
        if (storage is null || audioLibraryStorageService.Get(Context.Guild.Id.ToString()) is not ILibraryStorage<byte[]> libraryStorage)
        {
            await ReplyAsync("Sorry, there is no audio library available for this guild into which I could import the audio.");
            return;
        }

        subcommand = subcommand.ToLower();

        if (subcommand == "list" || subcommand == "ls")
        {
            LibraryStorageEntry<byte[]>[] libraryStorageEntries = libraryStorage.GetValues();

            if (libraryStorageEntries.Length == 0)
            {
                await ReplyAsync("No entries found in this guild's audio library");
                return;
            }

            List<EmbedFieldBuilder> embedFields = new List<EmbedFieldBuilder>();
            foreach (LibraryStorageEntry<byte[]> libraryStorageEntry in libraryStorageEntries)
            {
                EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder().WithName(libraryStorageEntry.Title);
                StringBuilder valueBuilder = new StringBuilder();

                if (libraryStorageEntry.Description is not null)
                    valueBuilder.AppendLine(libraryStorageEntry.Description);

                valueBuilder.AppendLine($"ID: {libraryStorageEntry.Id}");

                if (libraryStorageEntry.Tags is not null)
                    valueBuilder.AppendLine($"Tags: {string.Join(", ", libraryStorageEntry.Tags)}");

                if (libraryStorageEntry.Extras is not null)
                    foreach (KeyValuePair<string, string> extraPair in libraryStorageEntry.Extras)
                        valueBuilder.AppendLine($"{extraPair.Key}: {extraPair.Value}");

                embedFieldBuilder.WithValue(valueBuilder.ToString());
                embedFields.Add(embedFieldBuilder);
            }

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Audios of this guild");
            embedBuilder.WithColor(Color.Red);
            embedBuilder.WithFields(embedFields);
            await ReplyAsync(embed: embedBuilder.Build());

        }


        else if (subcommand == "import" || subcommand == "add")
        {
            IUserMessage message = await ReplyAsync("Importing media...");

            try
            {
                await WebUtils.ImportToLibraryStorageAsync(libraryStorage, url, title, description, tags, extras);
                await message.ModifyAsync(message => message.Content = "Media has been added to guild's audio library");
            }
            catch (Exception e)
            {
                if (!(e is ArgumentException || e is HttpRequestException))
                    Log.Error(e, "Error while trying to import media from {url} into AudioLibraryStorage of guild {guild}.", url, Context.Guild.Id.ToString());
                await message.ModifyAsync(message => message.Content = $"Error while importing media: {e.Message}");
            }
        }


        else
            await ReplyAsync($"Unknown subcommand: {subcommand}");
    }

}