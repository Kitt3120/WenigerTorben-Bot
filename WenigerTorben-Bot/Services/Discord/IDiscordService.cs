using Discord;
using Discord.Commands;

namespace WenigerTorbenBot.Services.Discord;

public interface IDiscordService : IService, IDiscordClient
{
    public CommandService GetCommandService();

}