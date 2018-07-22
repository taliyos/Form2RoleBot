using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBot
{
    internal class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        private const string Version = "0.1.0";

        private static void Main(string[] args)
            => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {
            StartArgsHandler(args);

            Console.WriteLine("Bot Token: " + Config.Bot.Token);
            Console.WriteLine("Bot Prefix: " + Config.Bot.Prefix);

            if (string.IsNullOrEmpty(Config.Bot.Token))
            {
                return;
            }
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, Config.Bot.Token);
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(1000);
            await Sheets.UpdateRoles(_client); // forces update initially on all servers
            Console.WriteLine("Starting sheet checking loop");
            while (true)
            {
                await Task.Delay((int)Math.Pow(1000, 2.19)); // ~an hour between updates
                await Sheets.CheckSheets(_client);
            }
        }


        private static async Task Log(LogMessage message)
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
                    Console.WriteLine("Form2Role Bot v{0}", Version);
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
