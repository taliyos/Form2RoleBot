using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class DiscordSpecificHandler
    {
        public static async Task PMNewUser(SocketGuildUser user)
        {
            if (Config.Bot.PMUsers)
            {
                await Discord.UserExtensions.SendMessageAsync(user, Config.Bot.PMMessage);
            }
        }
    }
}
