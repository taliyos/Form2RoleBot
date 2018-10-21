using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Config
    {
        private const string ConfPath = "Config";
        private const string ConfFile = ConfPath + "/config.json";

        private const string GoogleConfFile = ConfPath + "/googleConfig.json";

        private const string RoleConfFile = ConfPath + "/roleGroups.json";
        private const string IDsFile = ConfPath + "/userIDs.json";

        public static BotConfig Bot;
        public static GoogleConfig GoogleData;
        public static RoleGroupConfig RoleGroup;
        public static DiscordIDs UserIDs;

        public static bool newBotConfig, newGoogleConfig, newRoleGroupConfig, newUserIDs;

        

        static Config()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            Console.WriteLine("Loading Configuration Directory...");
            CreateConfigDirectory();

            Console.WriteLine("Loading Bot Configuration...");
            CreateConfigFiles();

            Console.WriteLine("Loading Google Configuration...");
            CreateGoogleConfigFiles();

            Console.WriteLine("Loading Role Group Configuration...");
            CreateRoleGroupConfigFiles();

            Console.WriteLine("Loading User IDs...");
            //CreateDiscordIDs();
        }

        private static void CreateConfigFiles()
        {
            if (!File.Exists(ConfFile))
            {
                Bot = new BotConfig
                {
                    // Default values for BotConfig
                    Token = null,
                    Prefix = "!",
                    UpdateDelay = 60, // 1 hour default
                    PMUsers = true,
                    PMMessage = "Welcome to the server!"
                };

                newBotConfig = true;
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

        public static void WriteToConfig(string token, string prefix, int delay)
        {
            Bot = new BotConfig()
            {
                Token = token,
                Prefix = prefix,
                UpdateDelay = delay
            };

            string botJson = JsonConvert.SerializeObject(Bot, Formatting.Indented);
            File.WriteAllText(ConfFile, botJson);
        }


        private static void CreateDiscordIDs()
        {
            if (!File.Exists(IDsFile))
            {
                UserIDs = new DiscordIDs
                {
                    IDs = new string[1]
                };
                
                newUserIDs = true;
                string userJson = JsonConvert.SerializeObject(UserIDs, Formatting.Indented);
                File.WriteAllText(IDsFile, userJson);
            }
            else
            {
                string userJson = File.ReadAllText(IDsFile);
                try
                {
                    Bot = JsonConvert.DeserializeObject<BotConfig>(userJson);
                }
                catch
                {
                    File.Delete(IDsFile);
                    CreateDiscordIDs();
                }
            }
        }

        public static void AppendToIDs(string userID)
        {
            List<string> IDs = new List<string>();

            foreach (string id in UserIDs.IDs)
            {
                IDs.Add(id);
            }
            IDs.Add(userID);

            string userJson = JsonConvert.SerializeObject(Bot, Formatting.Indented);
            File.WriteAllText(IDsFile, userJson);
        }

        public static void WriteToGoogleConfig(string apiKey, string spreadsheetID, string range, int rolesStartAfter, int rolesEndBefore, int discordID, int nickname)
        {
            GoogleData = new GoogleConfig()
            {
                APIKey = apiKey,
                SheetsID = spreadsheetID,
                Range = range,
                RolesStartAfter = rolesStartAfter,
                RolesEndBefore = rolesEndBefore,
                DiscordIDField = discordID,
                NicknameField = nickname,
            };

            string botJson = JsonConvert.SerializeObject(GoogleData, Formatting.Indented);
            File.WriteAllText(GoogleConfFile, botJson);
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

                newGoogleConfig = true;
                // Default values for GoogleData

                string googleJson = JsonConvert.SerializeObject(GoogleData, Formatting.Indented);
                File.WriteAllText(GoogleConfFile, googleJson);
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


        private static void CreateRoleGroupConfigFiles()
        {
            if (!File.Exists(RoleConfFile))
            {
                RoleGroup = new RoleGroupConfig
                {
                    // Default values for RoleGroupConfig
                    Groups = new string[5]
                };

                newRoleGroupConfig = true;
                string roleJson = JsonConvert.SerializeObject(RoleGroup, Formatting.Indented);
                File.WriteAllText(RoleConfFile, roleJson);
            }
            else
            {
                string roleJson = File.ReadAllText(RoleConfFile);
                try
                {
                    RoleGroup = JsonConvert.DeserializeObject<RoleGroupConfig>(roleJson);
                }
                catch
                {
                    File.Delete(RoleConfFile);
                    CreateRoleGroupConfigFiles();
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

        public static string[] DeserializeRoleGroup(string group)
        {
            //string group = config.Groups[index];
            group = group.Replace(" ", "");
            return group.Split(',');
        }

    }

    public struct BotConfig
    {
        public string Token; // Get this from discordapp.com/developers (this is also where you can add the bot to a server)
        public string Prefix; // The character required to send a command
        public int UpdateDelay; // Delay between checking for updates (in minutes)
        public bool PMUsers; // Whether or not users will be PMed to fill the form upon joining the server.
        public string PMMessage; // Message sent to new users
    }

    public struct GoogleConfig // Get the API Key from console.developers.google.com and make a sheets api and key. Get the sheets id from the google sheet (make one from a google form)
    {
        public string APIKey;
        public string SheetsID;
        public string Range; // Range on the form ie. B2:L which gets all information from B2 to L (goes all the way down too)
        public int RolesStartAfter; // # of spots after the initial index in the range where the roles begin
        public int RolesEndBefore; // # of spots before the final index in the range where the last role lies
        public int DiscordIDField; // -1 is default (first field) any other is a point in the range (0, 1, 2, 3...)
        public int NicknameField; // -1 is default (last field) any other is point in range (0, 1, 2, 3...)
        //-2 for the nickname field = no nickname field
        // -2 DOES NOT WORK FOR THE DISCORD ID FIELD, WHICH IS REQUIRED.
    }

    public struct RoleGroupConfig
    {
        public string[] Groups; // Used for storing role groups
    }

    public struct DiscordIDs
    {
        public string[] IDs;
    }

}