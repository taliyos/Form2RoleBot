/* Form2Role Bot v0.0.0 -> v0.1.3  created by Talios0 (August 4th, 2018)
 * Form2Role Bot v0.2.0 and v0.2.1 created by Talios0 (Charles), dsong175 (Daniel), and Lawrence-O (Lawrence)
 * Check the project out on Github: https://github.com/talios0/Form2RoleBot
 * This program uses Newtonsoft's JSON, RogueExceptions' Discord.NET, and Google Sheets API v4
 */

using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        private const string Version = "0.3.0a3";

        private static void Main(string[] args)
            => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {
            Console.WriteLine("---------------------------------------------------------------------");
            Console.WriteLine("Form2Role Bot v" + Version);
            Console.WriteLine("v0.0.0 -> current created by Talios0");
            Console.WriteLine("v0.2.0 and v0.2.1 created by Talios0 (Charles), dsong175 (Daniel), and Lawrence-O (Lawrence)");
            Console.WriteLine("Check for updates at https://github.com/talios0/Form2RoleBot/releases");
            Console.WriteLine("----------------------------------------------------------------------\n\n");

            LogSeverity logS = LogSeverity.Info;

            if (StartArgsHandler(args)) logS = LogSeverity.Verbose;

            RequestConfig();
            RequestGoogleConfig();

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
            _handler.InitializeAsync(_client).Wait();

            Console.WriteLine("\n");
            await Sheets.UpdateRoles(_client); // forces update initially on all servers

            while (true)
            {
                await Task.Delay(Config.Bot.UpdateDelay * 60000); // delay in minutes
                await Sheets.CheckSheets(_client);
            }
        }

        private void RequestGoogleConfig()
        {
            if (Config.newGoogleConfig || Config.GoogleData.APIKey == null)
            {
                Console.WriteLine("Google configuration data wasn't found or is corrupt.\n\nPress enter to keep the default value.");
                int i = 0;

                string apiKey = "";

                while (string.IsNullOrWhiteSpace(apiKey))
                {
                    if (i != 0) Console.WriteLine("Press enter to keep the default value. You can also configure the APIKey manually by editing the googleConfig.json file.\nAdditional information on how to find the API Key can be found on this repository's Readme");

                    Console.Write("API Key: ");
                    apiKey = Console.ReadLine();
                    i = 1;

                    if (apiKey == "") break;
                }

                string spreadsheetId = "";
                while (string.IsNullOrWhiteSpace(spreadsheetId))
                {
                    if (i != 1) Console.WriteLine("Press enter to keep the default value. You can also manually configure the SheetsID in googleConfig.json.\nAdditional information can be found on the Github repository.");

                    Console.Write("Spreadsheet ID: ");
                    spreadsheetId = Console.ReadLine();
                    i = 2;

                    if (spreadsheetId == "") break;
                }

                string range = "";
                while (string.IsNullOrWhiteSpace(range))
                {
                    if (i != 2) Console.WriteLine("Press enter to keep the default value. You can also manually configure the Range in googleConfig.json.\nAdditional information can be found on the Github repository.");

                    Console.Write("Range: ");
                    range = Console.ReadLine();
                    i = 3;

                    if (range == "") break;
                }

                int rolesStartAfter = -5;
                while (rolesStartAfter == -5)
                {
                    if (i != 3) Console.WriteLine("Press enter to keep the default value. You can also manually configure RolesStartAfter in googleConfig.json.\nAdditional information can be found on the Github repository.");

                    Console.Write("Roles Start After: ");
                    string value = Console.ReadLine();
                    i = 4;

                    if (value == "") break;

                    int.TryParse(value, out int temp);
                    if (temp >= 0) rolesStartAfter = temp;
                }

                int rolesEndBefore = -5;
                while (rolesEndBefore == -5)
                {
                    if (i != 4) Console.WriteLine("Press enter to keep the default value. You can also manually configure RolesEndBefore in googleConfig.json.\nAdditional information can be found on the Github repository.");

                    Console.Write("Roles End Before: ");
                    string value = Console.ReadLine();
                    i = 5;

                    if (value == "") break;

                    int.TryParse(value, out int temp);
                    if (temp >= 0)
                    {
                        rolesEndBefore = temp;
                    }
                }

                int discordID = -5;
                while (discordID == -5)
                {
                    if (i != 5) Console.WriteLine("Press enter to keep the default value. You can also manually configure DiscordIDField in googleConfig.json.\nAdditional information can be found on the Github repository.");

                    Console.Write("Discord ID Field: ");
                    string value = Console.ReadLine();
                    i = 6;

                    if (value == "") break;

                    int.TryParse(value, out int temp);
                    if (temp >= -1)
                    {
                        discordID = temp;
                    }
                }

                int nickname = -5;
                while (nickname == -5)
                {
                    if (i != 6) Console.WriteLine("Press enter to keep the default value. You can also manually configure NicknameField in googleConfig.json.\nAdditional information can be found on the Github repository.");

                    Console.Write("Nickname Field: ");
                    string value = Console.ReadLine();
                    i = 7;

                    if (value == "") break;

                    int.TryParse(value, out int temp);
                    if (temp >= -2)
                    {
                        nickname = temp;
                    }
                }
                Config.WriteToGoogleConfig(apiKey, spreadsheetId, range, rolesStartAfter, rolesEndBefore, discordID, nickname);
            }

        }

        private static void RequestConfig()
        {
            if (Config.newBotConfig || Config.Bot.Token == null)
            {
                Console.WriteLine("Bot configuration data was not found or is corrupt.\n\nPress enter to keep the default value.");
                string token = "";
                int i = 0;
                while (string.IsNullOrWhiteSpace(token))
                {
                    if (i != 0)
                        Console.WriteLine("Please enter a bot token or configure it manually in the config.json file. Please refer to the readme for instructions on how to find the token.");

                    i = 1;
                    Console.Write("Bot Token: ");
                    token = Console.ReadLine();
                    if (token == "") break;
                }

                string prefix = "";
                while (string.IsNullOrWhiteSpace(prefix))
                {
                    if (i != 1)
                        Console.WriteLine("Please enter a single character to serve as the command prefix. Otherwise, enter one manually in config.json.");

                    i = 2;
                    Console.Write("Bot Prefix: ");
                    prefix = Console.ReadLine();

                    if (prefix.Length == 0)
                    {
                        prefix = Config.Bot.Prefix;
                        break;
                    }
                    else if (prefix.Length != 1) prefix = "";
                }

                int delay = 0;
                while (delay <= 0)
                {
                    if (i != 2)
                        Console.WriteLine("The delay between checking for updates from the attached Google Sheet, specified in minutes, must be greater than 0. This value can be changed manually in config.json.");

                    Console.Write("Delay (in minutes): ");
                    string value = Console.ReadLine();

                    i = 3;
                    if (value == "")
                    {
                        delay = Config.Bot.UpdateDelay;
                        break;
                    }

                    int.TryParse(value, out delay);
                }

                Config.WriteToConfig(token, prefix, delay);
            }
        }


        private static async Task Log(LogMessage message)
        {
            Console.WriteLine(message.Message);
        }

        private static bool StartArgsHandler(string[] args)
        {
            if (args.Length == 0)
            {
                return false;
            }


            bool stop = true;

            foreach (string s in args)
            {
                if (s.Equals("--version"))
                {
                    Console.WriteLine("---------------------------------------------------------------------");
                    Console.WriteLine("Form2Role Bot v{0}", Version);
                    Console.WriteLine("---------------------------------------------------------------------");
                }
                else if (s.Equals("--help"))
                {
                    Console.WriteLine("---------------------------------------------------------------------");
                    Console.WriteLine("Check the GitHub repository (https://github.com/talios0/Form2RoleBot) for help. If a bug is encontered, submit an issue on the repo.");
                    Console.WriteLine("---------------------------------------------------------------------");
                }
                else if (s.Equals("--who"))
                {
                    Console.WriteLine("---------------------------------------------------------------------");
                    Console.WriteLine("v0.0.0 -> current Created by Talios0");
                    Console.WriteLine("v0.2.0 and v0.2.1 created by Talios0 (Charles), dsong175 (Daniel), and Lawrence-O (Lawrence)");
                    Console.WriteLine("----------------------------------------------------------------------");
                }
                else if (s.Equals("--update"))
                {
                    Console.WriteLine("---------------------------------------------------------------------");
                    Console.WriteLine("Check for updates at https://github.com/talios0/Form2RoleBot/releases");
                    Console.WriteLine("---------------------------------------------------------------------");
                }
                else if (s.Equals("--all"))
                {
                    Console.WriteLine("---------------------------------------------------------------------");
                    Console.WriteLine("Form2Role Bot v" + Version);
                    Console.WriteLine("v0.0.0 -> current created by Talios0");
                    Console.WriteLine("v0.2.0 and v0.2.1 created by Talios0 (Charles), dsong175 (Daniel), and Lawrence-O (Lawrence)");
                    Console.WriteLine("Check for updates at https://github.com/talios0/Form2RoleBot/releases");
                    Console.WriteLine("----------------------------------------------------------------------\n\n");
                }
                else if (s.Equals("--verbose"))
                {
                    Console.WriteLine("Form2Role Bot v{0} is now running in verbose mode. It is recommended to run this through the command prompt.");
                    return true;
                }
            }
            if (stop) Environment.Exit(0);
            return false;
        }
    }
}
