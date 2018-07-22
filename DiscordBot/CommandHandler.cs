using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
			_service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
        }


        private async Task HandleCommandAsync(SocketMessage s)
        {
            SocketUserMessage message = s as SocketUserMessage;
            if (message == null)
            {
                return;
            }
			SocketCommandContext context = new SocketCommandContext(_client, message);
            int argPos = 0;
            if (message.HasStringPrefix(Config.Bot.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                IResult result = await _service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }
    }
}
