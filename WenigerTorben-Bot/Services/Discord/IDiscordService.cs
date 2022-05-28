using Discord;
using Discord.Commands;

namespace WenigerTorbenBot.Services.Discord;

public interface IDiscordService : IDiscordClient
{
    public CommandService GetCommandService();

}