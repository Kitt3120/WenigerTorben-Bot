using System.Threading.Tasks;
using Discord;
using WenigerTorbenBot.Audio.AudioSource;

namespace WenigerTorbenBot.Audio.Queueing
{
    public class AudioRequest
    {
        public IGuildUser Requestor { get; }
        public IVoiceChannel? VoiceChannel { get; }
        public ITextChannel OriginChannel { get; }
        public string Request { get; }
        public IAudioSource AudioSource { get; }

        public AudioRequest(IGuildUser requestor, IVoiceChannel? voiceChannel, ITextChannel originChannel, string request, IAudioSource audioSource)
        {
            Requestor = requestor;
            VoiceChannel = voiceChannel;
            OriginChannel = originChannel;
            Request = request;
            AudioSource = audioSource;
        }

        public IVoiceChannel? GetTargetChannel()
        {
            if (VoiceChannel is not null)
                return VoiceChannel;

            return Requestor.VoiceChannel;
        }
    }
}