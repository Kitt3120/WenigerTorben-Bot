using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Serilog;
using WenigerTorbenBot.Audio.AudioSource;
using WenigerTorbenBot.Audio.Queueing;
using WenigerTorbenBot.Services.Audio;

namespace WenigerTorbenBot.Modules.Text;

[Name("Audio")]
[Summary("Module to manage a guild's audio session")]
public class AudioModule : ModuleBase<SocketCommandContext>
{
    private readonly IAudioService audioService;

    public AudioModule(IAudioService audioService)
    {
        this.audioService = audioService;
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

        IAudioSource? audioSource = AudioSource.Create(request);
        if (audioSource is null)
        {
            await ReplyAsync("Sorry, I don't know how to handle that request.");
            return;
        }

        AudioRequest audioRequest = new AudioRequest(guildUser, null, textChannel, request, audioSource);
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

    //TODO: Skip and Remove command
}