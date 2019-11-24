using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

// DISCLAIMER: This application is being written hastily (I have other projects and this isn't really a priority, everything should still work though)


namespace Form2RoleBot_PS.DiscordBot
{
    public class BotController
    {
        private DiscordSocketClient _client;
        //private CommandHandler _handler;

        private const string Version = "1.0.0";
        private string accessToken;

        public void Start() {
            ConfigureSettings();
            ConnectToDiscord();
        }

        private void ConfigureSettings() {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });
            _client.Log += Log;
        }

        private void ConnectToDiscord() {
            _client.LoginAsync(TokenType.Bot, "NDkxNTgxNzI2Njc0NTE4MDM2.XXQe4w.N3iB4nZn7sp-S_Xevf4sjShUUEo").Wait();
            _client.StartAsync().Wait();

        }

        public void SetToken(string token) {
            accessToken = token;
        }

        public bool AssignRoles() {
            DiscordAPI api = new DiscordAPI(accessToken);
            ulong userID = api.GetUser();

            foreach (var guild in _client.Guilds) {
                try
                {
                    SocketGuildUser user = guild.GetUser(userID);
                    SocketRole role = guild.Roles.FirstOrDefault(x => x.Name == "Test");
                    user.AddRoleAsync(role);
                    return true;
                }
                catch (Exception e) { 
                    // User wasn't in that guild. This error can be ignored.
                }
            }

            return false;
        }

        public static async Task Log(LogMessage message) {
            Console.WriteLine(message.Message);
        }

        // Getters and Setters
        public DiscordSocketClient GetBot() {
            return _client;
        }
    }
}
