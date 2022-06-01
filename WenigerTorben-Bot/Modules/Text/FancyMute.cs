using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WenigerTorbenBot.Services.FancyMute;

namespace WenigerTorbenBot.Modules.Text;

[Name("FancyMute")]
[Summary("Module to mute users, but in a cooler way.")]
public class FancyMute : ModuleBase<SocketCommandContext>
{
    private readonly IFancyMuteService fancyMuteService;

    public FancyMute(IFancyMuteService fancyMuteService)
    {
        this.fancyMuteService = fancyMuteService;
    }

    [Name("FancyMute")]
    [Summary("Mutes, unmutes or toggles mute of a user")]
    [Alias(new string[] { "fm", "" })]
    public async Task FancyMuteCommand(string operation, IUser user)
    {
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

    [Name("Mute")]
    [Summary("Mutes a user")]
    public async Task MuteCommand(IUser user)
    {
        fancyMuteService.Mute(Context.Guild, user);
        await ReplyAsync("User muted");
    }

    [Name("Unmute")]
    [Summary("Unmutes a user")]
    public async Task UnmuteCommand(IUser user)
    {
        fancyMuteService.Mute(Context.Guild, user);
        await ReplyAsync("User unmuted");
    }

    [Name("ToggleMute")]
    [Summary("Toggles mute of a user")]
    public async Task ToggleMuteCommand(IUser user)
    {
        bool muted = fancyMuteService.Toggle(Context.Guild, user);
        await ReplyAsync(muted ? "User muted" : "User unmuted");
    }

}