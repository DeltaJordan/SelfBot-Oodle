// <copyright file="Program.cs" company="JordantheBuizel">
// Copyright 2017 JordantheBuizel.
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SelfBot.Extensions;
using SelfBot.Scripting;

namespace SelfBot
{
    /// <summary>
    /// The main program
    /// </summary>
    internal class Program
    {
        public static readonly Random Random = new Random();
        public static readonly string AppPath = Directory.GetParent(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath).FullName;
        public static DiscordSocketClient Client;
        public CommandService Commands;
        private IServiceProvider serviceProvider;

        private async Task Start()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 10
            });

            this.Commands = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            });

            this.serviceProvider = new ServiceCollection().BuildServiceProvider();

            Client.Log += Log;
            this.Commands.Log += Log;

            await this.InstallCommands();

            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(AppPath, "config.xml"));
            string token = string.Empty;

            token = doc.SelectNodes("/Settings/Token")[0].InnerText;

            Console.Out.WriteLine(token);

            await Client.LoginAsync(TokenType.User, token);
            await Client.StartAsync();

            Constants.SelfBotMainInstance = this;

            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            Client.MessageReceived += this.HandleCommand;

            // Discover all of the commands in this assembly and load them.
            await this.Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage msg)
        {
            SocketUserMessage message = msg as SocketUserMessage;

            if (message == null)
            {
                return;
            }

            if (message.Content.StartsWith("!:") && message.Author.Id == Client.CurrentUser.Id)
            {
                // TODO CommandService seems to be broken? 
                if (Math.Abs(100) < 0)
                {
                    // Create a Command Context
                    CommandContext context = new CommandContext(Client, message);

                    // Execute the command. (result does not indicate a return value,
                    // rather an object stating if the command executed succesfully)
                    IResult result = await this.Commands.ExecuteAsync(context, message.Content.Substring(2), this.serviceProvider);
                    if (!result.IsSuccess)
                    {
                        Console.Out.WriteLine(result.ErrorReason + $"\nCommand:\n{message.Content.Substring(2)}");
                    }
                }

                string[] parse = message.Content.Substring(2).Split(' ');

                switch (parse[0].ToLower())
                {
                    case "channelmarkov":
                        {
                            try
                            {
                                List<IMessage> messages = new List<IMessage>();

                                message.Channel.GetMessagesAsync(1000).ForEach(coll => messages.AddRange(coll));

                                List<string> messageContentList = new List<string>();

                                foreach (IMessage message1 in messages)
                                {
                                    messageContentList.Add(message1.Content);
                                }

                                File.WriteAllLines(Path.Combine(AppPath, "markov.txt"), messageContentList.ToArray());

                                ProcessStartInfo info = new ProcessStartInfo
                                {
                                    FileName = "py",
                                    Arguments = $"-3 \"{Path.Combine(Program.AppPath, "markov.py")}\"",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                };

                                Process markovProcess = Process.Start(info);

                                string output = markovProcess.StandardOutput.ReadToEnd();

                                Console.Out.WriteLine(output);

                                if (output.Length < 10 && output.Contains("None"))
                                {
                                    return;
                                }

                                while (!File.Exists(Path.Combine(AppPath, "output.txt")))
                                {
                                }

                                if (string.IsNullOrWhiteSpace(File.ReadAllText(Path.Combine(AppPath, "output.txt"))))
                                {
                                    await message.ModifyAsync(properties =>
                                    {
                                        properties.Content = "Markov creation failed!";
                                    });

                                    await Task.Delay(1000);

                                    await message.DeleteAsync();
                                }

                                await message.ModifyAsync(properties =>
                                {
                                    properties.Content = string.Empty;

                                    EmbedBuilder builder = new EmbedBuilder();
                                    builder.Title = $"Markov output for channel {message.Channel.Name}:";
                                    builder.Description = $"{File.ReadAllText(Path.Combine(AppPath, "output.txt")).Replace("@", string.Empty).Replace("\n", string.Empty)}";


                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);

                                if (string.IsNullOrWhiteSpace(File.ReadAllText(Path.Combine(AppPath, "output.txt"))))
                                {
                                    await message.ModifyAsync(properties =>
                                    {
                                        properties.Content = "Markov creation failed!";
                                    });

                                    await Task.Delay(1000);

                                    await message.DeleteAsync();
                                }
                            }
                            finally
                            {
                                if (File.Exists(Path.Combine(AppPath, "output.txt")))
                                {
                                    File.Delete(Path.Combine(AppPath, "output.txt"));
                                }
                            }
                        }
                        break;
                    case "ping":
                        {
                            DateTime datetime = DateTime.Now;

                            int timeInMilliseconds = datetime.Millisecond + datetime.Second * 1000;

                            await message.ModifyAsync(properties =>
                            {
                                properties.Content = $"Pong! Response time {timeInMilliseconds - (message.Timestamp.Millisecond + message.Timestamp.Second * 1000)}ms";
                            });
                        }
                        break;
                    case "purge":
                        {
                            if (int.TryParse(parse[1], out int result))
                            {
                                if (result > 5)
                                {
                                    await message.ModifyAsync(properties =>
                                    {
                                        properties.Content = string.Empty;

                                        EmbedBuilder builder = new EmbedBuilder { Description = "Request refused..." };
                                        builder.WithColor(System.Drawing.Color.Red.ToDiscordColor());
                                        properties.Embed = builder.Build();
                                    });

                                    return;
                                }

                                List<IMessage> messages = new List<IMessage>();

                                message.Channel.GetMessagesAsync().ForEach(coll => messages.AddRange(coll));

                                int deleted = 0;

                                foreach (IMessage cachedMessage in messages)
                                {
                                    if (cachedMessage.Id == message.Id || cachedMessage.Author.Id != message.Author.Id)
                                    {
                                        continue;
                                    }

                                    await cachedMessage.DeleteAsync();

                                    deleted++;

                                    if (deleted == result)
                                    {
                                        break;
                                    }
                                }

                                await message.ModifyAsync(properties =>
                                {
                                    properties.Content = string.Empty;

                                    EmbedBuilder builder = new EmbedBuilder {Description = "Request successful!"};
                                    builder.WithColor(System.Drawing.Color.Green.ToDiscordColor());
                                    properties.Embed = builder.Build();
                                });

                                await Task.Delay(1000);

                                await message.DeleteAsync();
                            }
                        }
                        break;
                    case "execute":
                    {
                            try
                            {
                                string outputType = parse[1];

                                string[] namespaces = null;
                                if (Regex.Match(message.Content, @"\<namespaces\>.*\<\\namespaces\>").Success)
                                {
                                    namespaces = message.Content.Substring(Regex.Match(message.Content, @"\<namespaces\>.*\<\\namespaces\>").Index).Replace("<namespaces>", string.Empty).Replace("<\\namespaces>", string.Empty).Split(':');
                                }

                                string code = message.Content.Substring(9 + 1 + parse[1].Length);

                                int endMatchPos = Regex.Match(code, @"```\n").Index + Regex.Match(code, @"```\n").Length;

                                code = code.Substring(endMatchPos);
                                code = code.Substring(0, Regex.Match(code, @"\n```").Index);

                                code = ScriptHelper.DynamicMethod(code);

                                if (namespaces != null)
                                {
                                    code = ScriptHelper.AddNamespace(code, namespaces);
                                }
                                
                                Assembly assembly = ScriptHelper.Compile(code);
                                Module module = assembly.GetModules()[0];
                                Type mType = module.GetType("Script.Script");
                                MethodInfo methodInfo = mType.GetMethod("DynamicMethod");

                                List<CompilerError> errors = ScriptHelper.Errors;

                                if (errors.Count > 0)
                                {
                                    bool hasErrors = false;
                                    string errorBuilder = string.Empty;
                                    errorBuilder += "Errors:\n";

                                    foreach (CompilerError compilerError in errors)
                                    {
                                        errorBuilder += $"{compilerError.ErrorNumber}. {compilerError.ErrorText} at {{Line {compilerError.Line}:Col {compilerError.Column}}}";

                                        if (!compilerError.IsWarning)
                                        {
                                            hasErrors = true;
                                        }
                                    }

                                    if (hasErrors)
                                    {
                                        await message.ModifyAsync(properties =>
                                        {
                                            properties.Content = string.Empty;

                                            EmbedBuilder builder = new EmbedBuilder { Description = "Errors caught!\n" + errorBuilder };
                                            builder.WithColor(System.Drawing.Color.Red.ToDiscordColor());
                                            properties.Embed = builder.Build();
                                        });
                                        return;
                                    }
                                }

                                await message.ModifyAsync(prop =>
                                {
                                    prop.Content = string.Empty;

                                    EmbedBuilder builder = new EmbedBuilder();
                                    builder.Color = System.Drawing.Color.Green.ToDiscordColor();
                                    builder.Description = methodInfo.Invoke(null, null).ToString();
                                    prop.Embed = builder.Build();
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine(ex);
                            }
                    }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
