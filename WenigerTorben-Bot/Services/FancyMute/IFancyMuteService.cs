using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace WenigerTorbenBot.Services.FancyMute;

public interface IFancyMuteService : IService
{
    public bool IsMuted(IGuild guild, IUser user);
    public void Mute(IGuild guild, IUser user);
    public void Unmute(IGuild guild, IUser user);
    public bool Toggle(IGuild guild, IUser user);
    public Task OnMessageReceived(SocketMessage socketMessage);
}