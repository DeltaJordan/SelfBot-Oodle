// <copyright file="Program.cs" company="JordantheBuizel">
// Copyright 2017 JordantheBuizel.
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>


using Humanizer;

namespace PMDODiscordBot
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
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
                string mess = e.Message.Text;

                if (mess.StartsWith(".oodle"))
                {

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

                if (mess.StartsWith(".pfp"))
                {
                    mess = mess.Replace(".pfp", string.Empty).Replace("@", string.Empty).Trim();
                    string[] param = new[]
                    {
                        mess.Split('#')[0],
                        mess.Split('#')[1]
                    };

                    if (e.Server.GetUser(param[0], Convert.ToUInt16(param[1])) != null)
                    {
                        WebClient wbclient = new WebClient();
                        Stream stream =
                            wbclient.OpenRead(new Uri(e.Server.GetUser(param[0], Convert.ToUInt16(param[1])).AvatarUrl));
                        Bitmap bitmap;
                        bitmap = new Bitmap(stream);
                        
                        bitmap.Save(Path.Combine(this.appPath, "pfp" + ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == bitmap.RawFormat.Guid).FilenameExtension.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).First().Trim('*').ToLower()));
                    }
                    else
                    {
                        Console.Out.WriteLine("Failed");
                    }
                }

                if (mess.StartsWith(".makeitdouble"))
                {
                    char[] vowelsLower = new[]
                    {
                        'a',
                        'e',
                        'i',
                        'o',
                        'u'
                    };

                    mess = mess.Replace(".makeitdouble", string.Empty).Trim();

                    foreach (char vowel in vowelsLower)
                    {
                        mess = mess.Replace(vowel.ToString(), vowel.ToString() + vowel);
                    }

                    e.Message.Edit(mess);
                }

                if (mess.StartsWith(".quote"))
                {
                    mess = mess.Replace("quote", string.Empty).Trim();
                    if (int.TryParse(mess, out int output))
                    {
                        e.Message.Edit(this.doc.SelectNodes($"//Quotes/{output.ToWords()}")[0].InnerText);
                    }
                    else if (mess.ToLower() == "random")
                    {
                        XmlNodeList nodes = this.doc.SelectNodes("//Quotes")[0].ChildNodes;
                        XmlNode luckyNode = nodes[new Random().Next(0, nodes.Count - 1)];
                        e.Message.Edit(luckyNode.InnerText);
                    }
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
                    Console.Out.WriteLine(
                        "Bot failed to connect! Please verify token is correct. Also, if asking for help, NEVER GIVE OUT THE TOKEN EVEN TO ME(JordantheBuizel {Not that I'd want it})!");
                    throw;
                }
            });
        }

        private static void Main(string[] args) => new Program().Start();
    }
}
