using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace WenigerTorbenBot.Services.Discord;

public interface IDiscordService : IService
{
    public CommandService GetCommandService();
    public DiscordSocketClient GetWrappedClient();

}