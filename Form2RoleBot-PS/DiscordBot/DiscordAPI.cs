using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;

namespace Form2RoleBot_PS.DiscordBot
{
    public class DiscordAPI
    {
        private string _token;

        public DiscordAPI(string token)
        {
            _token = token;
        }

        public ulong GetUser() {
            DiscordRestClient client = new DiscordRestClient(new DiscordRestConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            client.LoginAsync(TokenType.Bearer, _token).Wait();

            return client.CurrentUser.Id;
        }
    }
}
