using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace DiscordBot
{
    class Config
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
            CreateConfigDirectory();
            Console.WriteLine("Config directory found!");

            CreateConfigFiles();
            Console.WriteLine("Config files found!");

            CreateGoogleConfigFiles();
            Console.WriteLine("Google information found!");
        }

        private static void CreateConfigFiles()
        {
            if (!File.Exists(ConfFile))
            {
                Bot = new BotConfig();
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
                catch (Exception e)
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
                GoogleData = new GoogleConfig();
                string googleJson = JsonConvert.SerializeObject(GoogleData, Formatting.Indented);
                File.WriteAllText(GoogleConfFile, googleJson);
                Console.WriteLine("Google Sheets data not found.");
            }
            else
            {
                string googleJson = File.ReadAllText(GoogleConfFile);
                try
                {
                    GoogleData = JsonConvert.DeserializeObject<GoogleConfig>(googleJson);
                }
                catch (Exception e)
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
        public string Token;
        public string Prefix;
    }

    public struct GoogleConfig
    {
        public string APIKey;
        public string SpreadsheetID;
        public string Range;
        public string Delay;
    }
}