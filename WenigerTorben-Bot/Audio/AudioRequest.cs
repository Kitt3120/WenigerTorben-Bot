using System.Threading.Tasks;
using Discord;

namespace WenigerTorbenBot.Audio
{
    public class AudioRequest
    {
        public IGuildUser Requestor { get; }
        public IVoiceChannel? VoiceChannel { get; }
        public ITextChannel OriginChannel { get; }
        public string Request { get; }

        public AudioRequest(IGuildUser requestor, IVoiceChannel? voiceChannel, ITextChannel originChannel, string request)
        {
            Requestor = requestor;
            VoiceChannel = voiceChannel;
            OriginChannel = originChannel;
            Request = request;
        }

        public async Task<IVoiceChannel?> GetTargetChannelAsync()
        {
            if (VoiceChannel is not null)
                return VoiceChannel;

            await Requestor.Guild.DownloadUsersAsync(); //TODO: Check if this is actually needed
            return Requestor.VoiceChannel;
        }
    }
}