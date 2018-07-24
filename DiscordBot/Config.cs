using System;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Config
    {
        private const string ConfPath = "Config";
        private const string ConfFile = ConfPath + "/config.json";

        private const string GoogleConfFile = ConfPath + "/googleConfig.json";

        public static BotConfig Bot;
        public static GoogleConfig GoogleData;

        

        static Config()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            Console.WriteLine("Loading Configuration Directory...");
            CreateConfigDirectory();

            Console.WriteLine("Loading Bot Configuration file...");
            CreateConfigFiles();

            Console.WriteLine("Loading Google Configuration File...");
            CreateGoogleConfigFiles();
        }

        private static void CreateConfigFiles()
        {
            if (!File.Exists(ConfFile))
            {
                Bot = new BotConfig
                {
                    // Default values for BotConfig
                    Prefix = "!",
                    UpdateDelay = 3600 // 1 hour default
                };

                string botJson = JsonConvert.SerializeObject(Bot, Formatting.Indented);
                File.WriteAllText(ConfFile, botJson);
            }
            else
            {
                string botJson = File.ReadAllText(ConfFile);
                try
                {
                    Bot = JsonConvert.DeserializeObject<BotConfig>(botJson);
                }
                catch
                {
                    File.Delete(ConfFile);
                    CreateConfigFiles();
                }
            }
        }

        private static void CreateGoogleConfigFiles()
        {
            if (!File.Exists(GoogleConfFile))
            {
                GoogleData = new GoogleConfig
                {
                    RolesStartAfter = 1,
                    RolesEndBefore = 1,
                    DiscordIDField = 0,
                    NicknameField = -1
                };

                // Default values for GoogleData

                string googleJson = JsonConvert.SerializeObject(GoogleData, Formatting.Indented);
                File.WriteAllText(GoogleConfFile, googleJson);
                Console.WriteLine("Google Sheets data not found or corrupt.");
            }
            else
            {
                string googleJson = File.ReadAllText(GoogleConfFile);
                try
                {
                    GoogleData = JsonConvert.DeserializeObject<GoogleConfig>(googleJson);
                }
                catch
                {
                    File.Delete(GoogleConfFile);
                    CreateGoogleConfigFiles();
                }
            }
        


        }

        private static void CreateConfigDirectory()
        {
            if (!Directory.Exists(ConfPath))
            {
                Directory.CreateDirectory(ConfPath); //Creates a config folder if it doesn't exist
            }
        }

    }

    public struct BotConfig
    {
        public string Token; // Get this from discordapp.com/developers (this is also where you can add the bot to a server)
        public string Prefix; // The character required to send a command
        public int UpdateDelay; // Delay between checking for updates (in seconds)
    }

    public struct GoogleConfig // Get the API Key from console.developers.google.com and make a sheets api and key. Get the sheets id from the google sheet (make one from a google form)
    {
        public string APIKey;
        public string SpreadsheetID;
        public string Range; // Range on the form ie. B2:L which gets all information from B2 to L (goes all the way down too)
        public int RolesStartAfter; // # of spots after the initial index in the range where the roles begin
        public int RolesEndBefore; // # of spots before the final index in the range where the last role lies
        public int DiscordIDField; // -1 is default (first field) any other is a point in the range (0, 1, 2, 3...)
        public int NicknameField; // -1 is default (last field) any other is point in range (0, 1, 2, 3...)
        //-2 for the nickname field = no nickname field
        // -2 DOES NOT WORK FOR THE DISCORD ID FIELD, WHICH IS REQUIRED.
    }
}