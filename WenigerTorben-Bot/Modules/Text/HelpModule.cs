using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WenigerTorbenBot.Services.Discord;

namespace WenigerTorbenBot.Modules.Text;

[Name("Help")]
[Summary("A module that provides commands to give instructions and further help to users")]
public class HelpModule : ModuleBase<SocketCommandContext>
{
    private IDiscordService discordService;
    public HelpModule(IDiscordService discordService)
    {
        this.discordService = discordService;
    }

    [Command("help")]
    [Alias(new string[]{"h", "?"})]
    [Summary("Prints a list of available commands or shows in-depth help for a specific command")]
    public async Task HelpCommand([Summary("The command to get in-depth help for")] string? command = null)
    {
        if(command is null)
        {
            List<EmbedBuilder> embedBuilders = new List<EmbedBuilder>();
            foreach(ModuleInfo moduleInfo in discordService.GetCommandService().Modules.OrderBy(module => module.Name))
            {
                EmbedBuilder embedBuilder = new EmbedBuilder();
                embedBuilder
                .WithTitle(moduleInfo.Name)
                .WithDescription($"{moduleInfo.Summary}{Environment.NewLine}{Environment.NewLine}Commands:")
                .WithColor(Color.Red);

                bool optionalParameters = false;
                List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();
                foreach(CommandInfo commandInfo in moduleInfo.Commands.OrderBy(command => command.Name))
                {
                    if(commandInfo.Parameters.Any(parameter => parameter.IsOptional))
                        optionalParameters = true;

                    StringBuilder nameBuilder = new StringBuilder();
                    nameBuilder.Append(commandInfo.Name);
                    if(commandInfo.Parameters.Count > 0)
                        nameBuilder.Append($" ({string.Join(", ", commandInfo.Parameters.Select(parameter => $"{parameter.Name}{(parameter.IsOptional ? "*" : string.Empty)}: {parameter.Type.Name}"))})");
                    if(commandInfo.Aliases.Count > 1)
                        nameBuilder.Append($" [{string.Join(", ", commandInfo.Aliases.Where(alias => alias != commandInfo.Name))}]");

                    EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder();
                    fieldBuilder
                    .WithName(nameBuilder.ToString())
                    .WithValue($"> {commandInfo.Summary}");
                    fieldBuilders.Add(fieldBuilder);
                }
                embedBuilder.WithFields(fieldBuilders);
                
                if(optionalParameters)
                    embedBuilder.WithFooter("* means optional parameter");

                embedBuilders.Add(embedBuilder);
            }

            await ReplyAsync(embeds: embedBuilders.Select(embedBuilder => embedBuilder.Build()).ToArray());
        }
        else
        {
            ModuleInfo? module = null;
            CommandInfo? cmd = null;
            foreach(ModuleInfo moduleInfo in discordService.GetCommandService().Modules)
                foreach(CommandInfo commandInfo in moduleInfo.Commands)
                    if(commandInfo.Aliases.Select(alias => alias.ToLower()).Contains(command.ToLower()))
                    {
                        module = moduleInfo;
                        cmd = commandInfo;
                    }
                        
            
            if(cmd is null)
            {
                IUserMessage userMessage = await ReplyAsync($"Command \"{command}\" not found");
                await Context.Message.AddReactionAsync(Emoji.Parse(":question:"));
                return;
            }

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle(cmd.Name);
            embedBuilder.WithDescription($"{cmd.Name} is a command from the {module.Name}-Module.{Environment.NewLine}{Environment.NewLine}Description: {cmd.Summary}{Environment.NewLine}Aliases: {(cmd.Aliases.Count > 1 ? string.Join(", ", cmd.Aliases.Where(alias => alias != cmd.Name)) : "None")}{Environment.NewLine}{Environment.NewLine}{(cmd.Parameters.Count > 0 ? "Parameters:" : "This command does not take any parameters")} ");

            if(cmd.Parameters.Count > 0)
            {
                List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();

                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    ParameterInfo parameterInfo = cmd.Parameters[i];

                    EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder();
                    embedFieldBuilder
                    .WithName($"{i+1}. {parameterInfo.Name}: {parameterInfo.Type.Name}{(parameterInfo.IsOptional ? " (Optional)" : string.Empty)}")
                    .WithValue($"> {(string.IsNullOrWhiteSpace(parameterInfo.Summary) ? "No description" : parameterInfo.Summary)}");
                    embedFieldBuilders.Add(embedFieldBuilder);
                }

                embedBuilder.WithFields(embedFieldBuilders);
            }

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}