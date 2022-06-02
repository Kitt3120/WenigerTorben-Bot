using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WenigerTorbenBot.Services.FancyMute;

namespace WenigerTorbenBot.Modules.Text;

[Name("FancyMute")]
[Summary("Module to mute users, but in a cooler way.")]
public class FancyMuteModule : ModuleBase<SocketCommandContext>
{
    private readonly IFancyMuteService fancyMuteService;

    public FancyMuteModule(IFancyMuteService fancyMuteService)
    {
        this.fancyMuteService = fancyMuteService;
    }

    [Command("fancymute")]
    [Alias(new string[] { "fm", "fancym", "fmute" })]
    [Summary("Mutes, unmutes or toggles mute of a user")]
    public async Task FancyMuteCommand(string operation, IUser user)
    {
        if (Context.Guild is null)
        {
            await ReplyAsync("This command is only available on servers.");
            return;
        }

        switch (operation.ToLower())
        {
            case "mute":
                await MuteCommand(user);
                break;
            case "unmute":
                await UnmuteCommand(user);
                break;
            case "toggle":
                await ToggleMuteCommand(user);
                break;
            default:
                await ReplyAsync($"Unknown operation: {operation}. Valid operations are mute, unmute and toggle.");
                break;
        }
    }

    [Command("mute")]
    [Summary("Mutes a user")]
    public async Task MuteCommand(IUser user)
    {
        if (Context.Guild is null)
        {
            await ReplyAsync("This command is only available on servers.");
            return;
        }

        await ReplyAsync("User muted");
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            fancyMuteService.Mute(Context.Guild, user);
        });
    }

    [Command("unmute")]
    [Summary("Unmutes a user")]
    public async Task UnmuteCommand(IUser user)
    {
        if (Context.Guild is null)
        {
            await ReplyAsync("This command is only available on servers.");
            return;
        }

        await ReplyAsync("User unmuted");
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            fancyMuteService.Unmute(Context.Guild, user);
        });
    }

    [Command("togglemute")]
    [Summary("Toggles mute of a user")]
    public async Task ToggleMuteCommand(IUser user)
    {
        if (Context.Guild is null)
        {
            await ReplyAsync("This command is only available on servers.");
            return;
        }

        bool muted = fancyMuteService.IsMuted(Context.Guild, user);
        await ReplyAsync(muted ? "User muted" : "User unmuted");
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            fancyMuteService.Toggle(Context.Guild, user);
        });
    }

}