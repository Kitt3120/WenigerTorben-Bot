using Discord;

namespace WenigerTorbenBot.Audio
{
    public class AudioRequest
    {
        public IUser Requestor { get; }
        public IAudioChannel? Channel { get; }
        public string Request { get; }

        public AudioRequest(IUser requestor, IAudioChannel? channel, string request)
        {
            Requestor = requestor;
            Channel = channel;
            Request = request;
        }
    }
}