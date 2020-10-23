using Discord;
using Discord.WebSocket;
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

        public static async Task VerifyUser(SocketGuild guild, SocketGuildUser user)
        {
            if (Config.Bot.PMSuccess == "") return;
            string text = Config.Bot.PMSuccess;
            text = text.Replace("/g", "**" + guild.Name + "**");
            text = text.Replace("/u", "**" + user.Username + "#" + user.Discriminator + "**");
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(guild.Name);
            embed.WithThumbnailUrl(guild.IconUrl);
            embed.WithDescription(text);
            embed.WithColor(Color.Green);
            await Discord.UserExtensions.SendMessageAsync(user, null , false, embed.Build());
        }
    }
}
