using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Modules;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient Client;
        private CommandHandler Handler;

        private const string Version = "0.0.3";

        private static void Main(string[] args)
            => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {
            StartArgsHandler(args);

            Console.WriteLine("Bot Token: " + Config.Bot.Token);
            Console.WriteLine("Bot Prefix: " + Config.Bot.Prefix);

            if (String.IsNullOrEmpty(Config.Bot.Token))
            {
                return;
            }
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            Client.Log += Log;
            await Client.LoginAsync(TokenType.Bot, Config.Bot.Token);
            await Client.StartAsync();
            Handler = new CommandHandler();
            await Handler.InitializeAsync(Client);
            await Task.Delay(1000);
            Sheets sheets = new Sheets();
            await sheets.UpdateRoles(Client);
            await Task.Delay(-1);
        }


        private async Task Log(LogMessage message)
        {
            Console.WriteLine(message.Message);
        }

        private static void StartArgsHandler(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }


            foreach (string s in args)
            {
                if (s.Equals("--version"))
                {
                    Console.WriteLine("Form2Role v{0}", Version);
                }
                else if (s.Equals("--help"))
                {
                    Console.WriteLine("Check the GitHub repository for help. If a bug is encontered, submit an issue on the repo.");
                }
                else if (s.Equals("--who"))
                {
                    Console.WriteLine("Created by Talios0");
                }
            }
            Environment.Exit(0);
        }
    }
}
