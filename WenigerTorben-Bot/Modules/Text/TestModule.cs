using System.Threading.Tasks;
using Discord.Commands;

namespace WenigerTorbenBot.Modules.Text;

public class TestModule : ModuleBase<SocketCommandContext>
{
    [Command("test")]
    [Summary("Tests if the bot is working or not")]
    public async Task TestCommand()
    {
        await ReplyAsync("Working!");
    }
}