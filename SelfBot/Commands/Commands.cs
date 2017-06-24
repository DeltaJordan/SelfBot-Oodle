using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
// ReSharper disable UnusedMember.Global

namespace SelfBot.Commands
{
    class Commands : ModuleBase
    {
        [Command("help"), Summary("Displays help")]
        public async Task Help(
            [Summary("Optional command to get help for")]
            string text = null)
        {
            if (text == null)
            {
                string helpbuilder = Constants.SelfBotMainInstance.Commands.Commands.Aggregate(string.Empty, (current, commandsCommand) => current + $"{commandsCommand.Name}, ");

                await this.Context.Message.ModifyAsync(properties =>
                {
                    properties.Content = string.Empty;

                    properties.Embed = new EmbedBuilder { Description = helpbuilder }.Build();
                });
            }
            else if (Constants.SelfBotMainInstance.Commands.Commands.ToList().Find(e => e.Name == text) != null)
            {
                CommandInfo requestedInfo = Constants.SelfBotMainInstance.Commands.Commands.ToList().Find(e => e.Name == text);

                string helpBuilder = $"```\n{text} ";

                if (requestedInfo.Parameters.Count > 0)
                {
                    helpBuilder = requestedInfo.Parameters.Aggregate(helpBuilder, (current, parameter) => current + $"[{parameter.Name}{(parameter.IsOptional ? $" = {Convert.ToString(parameter.DefaultValue)}" : string.Empty)}] ");
                }

                if (requestedInfo.Aliases.Count > 0)
                {
                    helpBuilder += "\nAliases: ";
                    helpBuilder = requestedInfo.Aliases.Aggregate(helpBuilder, (current, alias) => current + $"{alias} ");
                }

                if (!string.IsNullOrWhiteSpace(requestedInfo.Summary))
                {
                    helpBuilder += $"\n{requestedInfo.Summary}\n";
                }

                if (requestedInfo.Parameters.Count > 0)
                {
                    helpBuilder = requestedInfo.Parameters.Where(parameter => !string.IsNullOrWhiteSpace(parameter.Summary)).Aggregate(helpBuilder, (current, parameter) => current + $"\n{parameter.Name}: {parameter.Summary}");
                }

                await this.ReplyAsync($"{helpBuilder}\n```");
            }
            else
            {
                Dictionary<string, string> allAliasDict = new Dictionary<string, string>();

                foreach (CommandInfo cmdInfo in Constants.SelfBotMainInstance.Commands.Commands)
                {
                    foreach (string cmdInfoAlias in cmdInfo.Aliases)
                    {
                        allAliasDict.Add(cmdInfoAlias, cmdInfo.Name);
                    }
                }

                if (allAliasDict.TryGetValue(text, out string result))
                {
                    CommandInfo requestedInfo = Constants.SelfBotMainInstance.Commands.Commands.ToList().Find(e => e.Name == result);

                    string helpBuilder = $"```\n{result} ";

                    if (requestedInfo.Parameters.Count > 0)
                    {
                        foreach (ParameterInfo parameter in requestedInfo.Parameters)
                        {
                            helpBuilder += $"[{parameter.Name}{(parameter.IsOptional ? $" = {Convert.ToString(parameter.DefaultValue)}" : string.Empty)}] ";
                        }
                    }

                    if (requestedInfo.Aliases.Count > 0)
                    {
                        helpBuilder += $"\nAliases: ";
                        helpBuilder = requestedInfo.Aliases.Aggregate(helpBuilder, (current, alias) => current + $"{alias} ");
                    }

                    if (!string.IsNullOrWhiteSpace(requestedInfo.Summary))
                    {
                        helpBuilder += $"\n{requestedInfo.Summary}\n";
                    }

                    if (requestedInfo.Parameters.Count > 0)
                    {
                        foreach (ParameterInfo parameter in requestedInfo.Parameters)
                        {
                            if (!string.IsNullOrWhiteSpace(parameter.Summary))
                            {
                                helpBuilder += $"\n{parameter.Name}: {parameter.Summary}";
                            }
                        }
                    }

                    await this.ReplyAsync($"{helpBuilder}\n```");
                }
                else
                {
                    await this.ReplyAsync("Command not found!");
                }
            }
        }
    }
}
