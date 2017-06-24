using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SelfBot.Extensions
{
    public static class SystemColorToDiscord
    {
        public static Color ToDiscordColor(this System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B);
        }
    }
}
