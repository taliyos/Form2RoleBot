using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Sheets
    {

        private static readonly string SheetId = Config.GoogleData.SheetsID;
        private static readonly string Range = Config.GoogleData.Range;

        private static SheetsService _service;

        private static IList<IList<object>> _previousSheetValues;
        private static IList<object> _columnNames;

        // Requests Google Sheets information and initiates command to assign roles
        public static async Task UpdateRoles(DiscordSocketClient client)
        {
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = Config.GoogleData.APIKey,
                ApplicationName = "Form2Role Bot"
            });

            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(SheetId, Range);
            string start = Range.Split(':')[0];
            start = Regex.Replace(start, @"[\d-]", string.Empty);
            string end = Range.Split(':')[1];
            end = Regex.Replace(end, @"[\d-]", string.Empty);
            SpreadsheetsResource.ValuesResource.GetRequest columns = _service.Spreadsheets.Values.Get(SheetId, start+"1:"+end+"1");


            ValueRange responses = request.Execute();
            ValueRange columnNames = columns.Execute();
            _previousSheetValues = responses.Values;
            _columnNames = columnNames.Values[0];

            await AssignRoles(client, _previousSheetValues);
        }

        private static async Task AssignRoles(DiscordSocketClient client, IList<IList<Object>> values)
        {
            Stopwatch time = Stopwatch.StartNew();
            if (values != null && values.Count > 0)
            {
                foreach (SocketGuild g in client.Guilds) // Updates for every guild the bot is registered in. Take note that this will quickly hit a discord limit. This is fine, it will resume after a few seconds.
                {
                    Dictionary<SocketGuildUser, IList<object>> redoUsers = new Dictionary<SocketGuildUser, IList<object>>();

                    Console.WriteLine("\n\nUpdating roles in " + g.Name + ".");
                    await g.DownloadUsersAsync(); // Gets all users
                    SocketGuildUser[] allUsers = g.Users.ToArray(); // Converts list of users to array

                    foreach (SocketGuildUser u in allUsers)
                    {
                        // Checking roles for user
                        foreach (IList<object> row in values)
                        {
                            if (!SheetsFunctionality.FindUsername(u, row)) continue;

                            //await SheetsFunctionality.StoreUserID(u);
                            if (Config.GoogleData.NicknameOnly)
                            {
                                Console.WriteLine("Updating Nickname for " + u.Username + "#" + u.Discriminator);
                                await SheetsFunctionality.FindAndSetNickname(u, row);
                                continue;
                            }

                            Console.WriteLine("Updating Roles for " + u.Username + "#" + u.Discriminator);
                            List<string> allUserRoles = new List<string>(); // All of the rolls that need to be assigned to the user

                            // Gets all roles that need to be assigned to the user in addition to removing those that interfere with roleGroups.json
                            allUserRoles = await SheetsFunctionality.GetRoles(row, u);
                            if(Config.Bot.AutoRole != "") allUserRoles.Add(Config.Bot.AutoRole);

                            List<SocketRole> formattedRoles = new List<SocketRole>();

                            bool redo = false;
                            // Google Sheets data to SocketRole
                            foreach (string s in allUserRoles)
                            {
                                SocketRole role = g.Roles.FirstOrDefault(x => x.Name == s);
                                if (role == default(SocketRole))
                                {
                                    role = await SheetsFunctionality.CreateRole(g, s);
                                    redo = true;
                                }
                                formattedRoles.Add(role); // Adds role to list of queued roles
                            }

                            // Add user to list of roles to redo
                            if (redo) redoUsers.Add(u, row);

                            // Add Roles To User
                            await SheetsFunctionality.AddRolesToUser(u, formattedRoles.ToArray());

                            // Find and set nickname
                            await SheetsFunctionality.FindAndSetNickname(u, row);
                        }

                        // Secondary Role Assigner for roles that were just created
                        await AssignNewRoles(g, redoUsers);
                    }
                }

            }
            time.Stop();
            Console.WriteLine(time.ElapsedMilliseconds + "ms\n");
        }

        public static async Task AssignNewRoles(SocketGuild guild, Dictionary<SocketGuildUser, IList<object>> users)
        {
            foreach (KeyValuePair<SocketGuildUser, IList<object>> user in users)
            {
                List<string> allRoles = await SheetsFunctionality.GetRoles(user.Value, user.Key);
                List<SocketRole> mappedRoles = new List<SocketRole>();
                foreach (string s in allRoles)
                {
                    SocketRole role = guild.Roles.FirstOrDefault(x => x.Name == s);
                    mappedRoles.Add(role);
                }

                await SheetsFunctionality.AddRolesToUser(user.Key, mappedRoles.ToArray());
            }
        }

        public static async Task CheckSheets(DiscordSocketClient client)
        {

            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(SheetId, Range);

            ValueRange responses = request.Execute();
            IList<IList<Object>> values = responses.Values;
            string start = Range.Split(':')[0];
            start = Regex.Replace(start, @"[\d-]", string.Empty);

            // Checks if the newly retrieved sheet is the same as the previous one
            if (values.Count == _previousSheetValues.Count)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    for (int x = 0; x < values[i].Count; x++)
                    {
                        if (!values[i][x].Equals(_previousSheetValues[i][x]))
                        {
                            await AssignRoles(client, values); // Re-assigns all roles (System doesn't know if someone changed their previous form results)
                            _previousSheetValues = values;
                            return;
                        }
                    }
                }
            }

            else
            {
                await AssignRoles(client, values);
                _previousSheetValues = values;
                return;
            }

            Console.WriteLine("[" + DateTime.Now + "] No update from the linked Google Sheet.");

        }

    }
}
