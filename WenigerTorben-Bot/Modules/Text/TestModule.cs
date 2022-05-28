using System.Threading.Tasks;
using Discord.Commands;
using WenigerTorbenBot.Modules.Attributes;

namespace WenigerTorbenBot.Modules.Text;

[Name("Test")]
[Summary("A module that provides commands to test the functionality of the bot")]
[Hidden()]
public class TestModule : ModuleBase<SocketCommandContext>
{
    [Command("test")]
    [Summary("Tests whether the command handler is working")]
    [Hidden()]
    public async Task TestCommand()
    {
        await ReplyAsync("Working!");
    }
}