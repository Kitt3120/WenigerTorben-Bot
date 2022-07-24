using System.Threading.Tasks;
using Discord.WebSocket;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Services.Storage.Config.Guild;

public interface IGuildConfigStorageService<T> : IConfigStorageService<T>
{
    internal void SynchronizeConfigs(DiscordSocketClient discordSocketClient);
    internal Task OnGuildJoin(SocketGuild socketGuild);
    internal Task OnGuildLeave(SocketGuild socketGuild);
}