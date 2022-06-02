using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot.Services.FancyMute;

public class FancyMuteService : Service, IFancyMuteService
{
    public override string Name => "FancyMute";
    public override ServicePriority Priority => ServicePriority.Optional;

    private readonly IDiscordService discordService;

    private readonly object mutedUsersLock;
    private readonly Dictionary<IGuild, List<IUser>> mutedUsers;
    private readonly string[] reactionMessages;
    private readonly Random random;

    private DiscordSocketClient? discordSocketClient;

    public FancyMuteService(IDiscordService discordService)
    {
        this.discordService = discordService;
        this.mutedUsersLock = new object();
        this.mutedUsers = new Dictionary<IGuild, List<IUser>>();
        this.reactionMessages = new string[] { "no", "nope", "cya", "ciao", "bye", "gone", "muted", "quiet", "ok thx", "ok nice", "ok cool", "X" };
        this.random = new Random();
    }

    protected override void Initialize()
    {
        IDiscordClient usedDiscordClient = discordService.GetWrappedClient();
        if (usedDiscordClient is not DiscordSocketClient socketClient)
            throw new Exception("The provided IDiscordClient implementation is no DiscordSocketClient");

        discordSocketClient = socketClient;
        discordSocketClient.MessageReceived += OnMessageReceived;
    }

    public bool IsMuted(IGuild guild, IUser user) => mutedUsers.ContainsKey(guild) && mutedUsers[guild].Contains(user);

    public void Mute(IGuild guild, IUser user)
    {
        lock (mutedUsersLock)
        {
            List<IUser> mutedList;
            if (mutedUsers.ContainsKey(guild))
                mutedList = mutedUsers[guild];
            else
            {
                mutedList = new List<IUser>();
                mutedUsers[guild] = mutedList;
            }

            if (!mutedList.Contains(user))
                mutedList.Add(user);
        }
    }

    public void Unmute(IGuild guild, IUser user)
    {
        lock (mutedUsersLock)
        {
            if (!mutedUsers.ContainsKey(guild))
                return;

            List<IUser> mutedList = mutedUsers[guild];

            if (!mutedList.Contains(user))
                mutedList.Remove(user);

            if (mutedList.Count == 0)
                mutedUsers.Remove(guild);
        }
    }

    public bool Toggle(IGuild guild, IUser user)
    {
        bool isMuted = IsMuted(guild, user);
        if (isMuted)
            Unmute(guild, user);
        else
            Mute(guild, user);
        return !isMuted;
    }

    public async Task OnMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage.Channel is not SocketGuildChannel socketGuildChannel || !IsMuted(socketGuildChannel.Guild, socketMessage.Author))
            return;

        string reactionMessage = reactionMessages[random.Next(reactionMessages.Length)];
        string[] reactions;
        try
        {
            reactions = ReactionUtils.FromString(reactionMessage);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to resolve reactions");
            return;
        }

        foreach (string reaction in reactions)
            await socketMessage.AddReactionAsync(Emoji.Parse(reaction));

        await Task.Delay(1000);
        await socketMessage.DeleteAsync();
    }

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();
}