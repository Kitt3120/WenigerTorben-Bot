using System.Threading.Tasks;
using Discord;
using WenigerTorbenBot.Audio.AudioSource;

namespace WenigerTorbenBot.Audio.Queueing
{
    public class AudioRequest : IAudioRequest
    {
        public IGuildUser Requestor { get; init; }
        public IVoiceChannel VoiceChannel { get; init; }
        public ITextChannel OriginChannel { get; init; }
        public string Request { get; init; }
        public IAudioSource AudioSource { get; init; }

        public AudioRequest(IGuildUser requestor, IVoiceChannel voiceChannel, ITextChannel originChannel, string request, IAudioSource audioSource)
        {
            Requestor = requestor;
            VoiceChannel = voiceChannel;
            OriginChannel = originChannel;
            Request = request;
            AudioSource = audioSource;
        }
    }
}