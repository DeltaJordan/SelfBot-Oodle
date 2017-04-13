// <copyright file="Program.cs" company="JordantheBuizel">
// Copyright (c) JordantheBuizel. All rights reserved.
// </copyright>

using SuperSocket.ClientEngine;

namespace PMDODiscordBot
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Discord;
    using Discord.Audio;
    using Discord.Commands;

    /// <summary>
    /// The main program
    /// </summary>
    internal class Program
    {
        private DiscordClient client;

        private string appPath = Directory.GetParent(new Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase).LocalPath).FullName;

        private XmlDocument doc;

        private bool foolsMode = DateTime.Now.Month == 4 && DateTime.Now.Day == 1;

        // private IAudioClient vClient;

        /// <summary>
        /// Ordered types of Pokémon, Compliant with PMDO SQL server
        /// </summary>
        public enum PokemonType
        {
            /// <summary>
            /// Type to designate error or no secondary type
            /// </summary>
            None,

            /// <summary>
            /// Bug type
            /// </summary>
            Bug,

            /// <summary>
            /// Dark type
            /// </summary>
            Dark,

            /// <summary>
            /// Dragon type
            /// </summary>
            Dragon,

            /// <summary>
            /// Electric type
            /// </summary>
            Electric,

            /// <summary>
            /// Fairy type
            /// </summary>
            Fairy,

            /// <summary>
            /// Fighting type
            /// </summary>
            Fighting,

            /// <summary>
            /// Fire type
            /// </summary>
            Fire,

            /// <summary>
            /// Flying type
            /// </summary>
            Flying,

            /// <summary>
            /// Ghost type
            /// </summary>
            Ghost,

            /// <summary>
            /// Grass type
            /// </summary>
            Grass,

            /// <summary>
            /// Ground type
            /// </summary>
            Ground,

            /// <summary>
            /// Ice type
            /// </summary>
            Ice,

            /// <summary>
            /// Normal type
            /// </summary>
            Normal,

            /// <summary>
            /// Poison type
            /// </summary>
            Poison,

            /// <summary>
            /// Psychic type
            /// </summary>
            Psychic,

            /// <summary>
            /// Rock type
            /// </summary>
            Rock,

            /// <summary>
            /// Steel type
            /// </summary>
            Steel,

            /// <summary>
            /// Water type
            /// </summary>
            Water
        }

        /// <summary>
        /// Bot's main method
        /// </summary>
        private void Start()
        {
            this.client = new DiscordClient();

            this.client.UsingCommands(x =>
            {
                x.PrefixChar = '.';
                x.HelpMode = HelpMode.Public;
            });

            this.client.MessageReceived += (s, e) =>
            {
                if (e.Message.Text.StartsWith(".oodle"))
                {
                    string mess = e.Message.Text;

                    mess = mess.Replace(".oodle", string.Empty);
                    char[] vowelsLower = new[]
                    {
                        'a',
                        'e',
                        'i',
                        'o',
                        'u'
                    };

                    mess = vowelsLower.Aggregate(mess, (current, vowel) => current.Replace(vowel, '~'));

                    this.doc.Load(Path.Combine(this.appPath, "config.xml"));
                    bool caseSensitive = Convert.ToBoolean(this.doc.SelectNodes("//CaseSensitive")[0].InnerText);

                    if (caseSensitive)
                    {
                        for (int index = 0; index < vowelsLower.Length; index++)
                        {
                            vowelsLower[index] = vowelsLower[index].ToString().ToUpper().ToCharArray()[0];
                        }
                    }

                    this.doc.Load(Path.Combine(this.appPath, "config.xml"));
                    string replace = this.doc.SelectNodes("//ReplaceChar")[0].InnerText;

                    mess = mess.Replace(replace, "oodle");

                    e.Message.Edit(mess);
                }
            };

            this.client.ExecuteAndWait(async () =>
            {
                try
                {
                    this.doc = new XmlDocument();
                    this.doc.Load(Path.Combine(this.appPath, "config.xml"));
                    string token = this.doc.SelectNodes("//Token")[0].InnerText;

                    await this.client.Connect(token, TokenType.User);
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine("Bot failed to connect! Please verify token is correct. Also, if asking for help, NEVER GIVE OUT THE TOKEN EVEN TO ME(JordantheBuizel {Not that I'd want it})!");
                    throw;
                }

                // var voiceChannel = _client.FindServers("PMD Online Staff/Volunteers").FirstOrDefault().VoiceChannels.FirstOrDefault(); // Finds the first VoiceChannel on the server 'Music Bot Server'

                // _vClient = await _client.GetService<AudioService>() // We use GetService to find the AudioService that we installed earlier. In previous versions, this was equivelent to _client.Audio()
                // .Join(voiceChannel); // Join the Voice Channel, and return the IAudioClient.
            });
        }

        private static void Main(string[] args) => new Program().Start();
    }
}
