using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog;
using WenigerTorbenBot.Audio.AudioSource;
using WenigerTorbenBot.Audio.Queueing;
using WenigerTorbenBot.Audio.Session;
using WenigerTorbenBot.Metadata;
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
    private readonly IAudioService audioService;
    private readonly AudioLibraryStorageService audioLibraryStorageService;

    public AudioModule(IAudioService audioService, AudioLibraryStorageService audioLibraryStorageService)
    {
        this.audioService = audioService;
        this.audioLibraryStorageService = audioLibraryStorageService;
    }

    [Command("play")]
    [Alias(new string[] { "p", "pl" })]
    [Summary("Enqueues an audio request")]
    public async Task Play(string? request = null)
    {
        if (Context.User is not IGuildUser guildUser || Context.Channel is not ITextChannel textChannel)
        {
            await ReplyAsync("This command is only available on guilds");
            return;
        }

        if (string.IsNullOrWhiteSpace(request))
        {
            await ReplyAsync("Sorry, you actually did not request any media I should play. Try again!");
            return;
        }

        IAudioSource? audioSource = AudioSource.Create(Context.Guild, request);
        if (audioSource is null)
        {
            await ReplyAsync($"Sorry {Context.User.Mention}, I don't know how to handle the given request");
            return;
        }

        SocketVoiceChannel? voiceChannel = Context.Guild.VoiceChannels.Where(vc => vc.ConnectedUsers.Any(user => user.Id == Context.User.Id)).FirstOrDefault();
        if (voiceChannel is null)
        {
            await ReplyAsync($"Sorry {Context.User.Mention}, your request could not be queued as I was not able to find the voice channel you're in");
            return;
        }

        AudioRequest audioRequest = new AudioRequest(guildUser, voiceChannel, textChannel, request, audioSource);
        int position = audioService.Enqueue(audioRequest);

        await ReplyAsync($"{Context.User.Mention}, your request has been added to the queue in position {position + 1}");
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

        audioService.Resume(Context.Guild);
        await ReplyAsync("Audio session resumed");
    }

    [Command("skip")]
    [Summary("Skips the current track")]
    public async Task Skip()
    {
        if (Context.User is not IGuildUser || Context.Channel is not ITextChannel)
        {
            await ReplyAsync("This command is only available on guilds");
            return;
        }

        IAudioSession audioSession = audioService.GetAudioSession(Context.Guild);
        if (audioSession.HasReachedEnd)
            await ReplyAsync($"{Context.User.Mention}, nothing is playing");
        else
        {
            audioSession.Skip();
            await ReplyAsync($"{Context.User.Mention}, skip requested");
        }
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
            await ReplyAsync("Sorry, there is no audio library available for this guild into which I could import the audio");
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
                EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder().WithName(libraryStorageEntry.Metadata.Title);
                StringBuilder valueBuilder = new StringBuilder();

                if (libraryStorageEntry.Metadata.Description is not null)
                    valueBuilder.AppendLine(libraryStorageEntry.Metadata.Description);

                valueBuilder.AppendLine($"ID: {libraryStorageEntry.Metadata.ID}");

                if (libraryStorageEntry.Metadata.Tags is not null)
                    valueBuilder.AppendLine($"Tags: {string.Join(", ", libraryStorageEntry.Metadata.Tags)}");

                if (libraryStorageEntry.Metadata.Extras is not null)
                    foreach (KeyValuePair<string, string> extraPair in libraryStorageEntry.Metadata.Extras)
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
            if (string.IsNullOrWhiteSpace(url))
            {
                await ReplyAsync("You did not specify the URL I should download the media from");
                return;
                //throw new ArgumentNullException(nameof(url), "Value was null or an empty string");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                await ReplyAsync("You did not specify a title for the imported media");
                return;
                //throw new ArgumentNullException(nameof(title), "Value was null or an empty string");
            }

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
                        await ReplyAsync("Syntax for one of the defined extra contained an error");
                        return;
                        //throw new ArgumentException("Syntax for one of the defined extras contained an error", nameof(extras));
                    }

                    extrasDictionary[extraPairSplit[0]] = extraPairSplit[1];
                }
            }

            IUserMessage message = await ReplyAsync("Importing media...");

            IAudioSource? audioSource = AudioSource.Create(Context.Guild, url);
            if (audioSource is null)
            {
                await ReplyAsync("No fitting AudioSource found for the given url");
                return;
            }

            await audioSource.WhenMetadataLoaded();
            IMetadata sourceMetadata = audioSource.GetAudioSourceMetadata();

            Metadata.Metadata metadata = new MetadataBuilder()
                                    .WithID(Guid.NewGuid().ToString())
                                    .WithTitle(title)
                                    .WithDescription(description ?? sourceMetadata.Description)
                                    .WithOrigin(sourceMetadata.Origin)
                                    .WithDuration(sourceMetadata.Duration)
                                    .WithAuthor(sourceMetadata.Author)
                                    .WithTags(tagsArray ?? sourceMetadata.Tags)
                                    .WithExtras(extrasDictionary ?? sourceMetadata.Extras)
                                    .Build();

            Log.Debug("Using AudioSource of type {audioSourceType} for request {url} by {user} on Guild {guild}.", audioSource.GetAudioSourceType(), url, Context.User.Id, Context.Guild.Id);

            try
            {
                await audioSource.WhenContentPrepared(true);
                using MemoryStream memoryStream = new MemoryStream();
                await audioSource.StreamAsync(memoryStream);
                await libraryStorage.ImportAsync(metadata, memoryStream.GetBuffer());
                await message.ModifyAsync(message => message.Content = "Audio has been added to the guild's library");
            }
            catch (Exception e)
            {
                if (e is not ArgumentException && e is not HttpRequestException)
                    Log.Error(e, "Error while trying to import media from {url} into AudioLibraryStorage of guild {guild}.", url, Context.Guild.Id.ToString());
                await message.ModifyAsync(message => message.Content = $"Sorry {Context.User.Mention}, an error occured while importing the audio: {e.Message}");
            }
        }


        else
            await ReplyAsync($"Sorry, you have entered an unknown subcommand: \"{subcommand}\"");
    }

}