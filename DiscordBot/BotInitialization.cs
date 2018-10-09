using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using Discord;
using Discord.WebSocket;

namespace DiscordBot
{
    public class BotInitialization
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public BotInitialization()
        {

        }

        public async Task CreateBot() // Put into seperate class to cooperate with the new GUI
        {
            Console.WriteLine("\n");
            Console.WriteLine("Bot Token: " + Config.Bot.Token);
            Console.WriteLine("Bot Prefix: " + Config.Bot.Prefix);

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });

            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, Config.Bot.Token);
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);

            //FindServers(servers);

            await Sheets.UpdateRoles(_client);

            while (true)
            {
                //FindServers(servers);
                await Task.Delay(Config.Bot.UpdateDelay * 60000); // delay in minutes
                await Sheets.CheckSheets(_client);
            }
        }

        public void FindServers(TextBlock servers)
        {
            servers.Text = "This works!";
        }

        private static async Task Log(LogMessage message)
        {
            Console.WriteLine(message.Message);
        }
    }
}