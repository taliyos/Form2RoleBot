using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace DiscordBot
{
    class Sheets
    {

        private static readonly string SheetId = Config.GoogleData.SheetsID;
        private static readonly string Range = Config.GoogleData.Range;

        private static SheetsService _service;

        private static IList<IList<object>> _previousSheetValues;

        public static async Task UpdateRoles(DiscordSocketClient client)
        {
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = Config.GoogleData.APIKey,
                ApplicationName = "Form2Role Bot"
            });

            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(SheetId, Range);

            ValueRange responses = request.Execute();
            _previousSheetValues = responses.Values;

            await AssignRoles(client, _previousSheetValues);

        }

        private static async Task AssignRoles(DiscordSocketClient client, IList<IList<Object>> values)
        {
            if (values != null && values.Count > 0)
            {
                foreach (SocketGuild g in client.Guilds) // Updates for every guild the bot is registered in. Take note that this will quickly hit a discord limit. This is fine, it will resume after a few seconds.
                {
                    Dictionary<SocketGuildUser,IList<object>> redoUsers = new Dictionary<SocketGuildUser,IList<object>>();
                    
                    Console.WriteLine("\n\nUpdating roles in " + g.Name + ".");
                    foreach (SocketGuildUser u in g.Users)
                    {
                        foreach (IList<object> row in values)
                        {
                            if (!SheetsFunctionality.FindUsername(u, row)) continue;

                            await SheetsFunctionality.StoreUserID(u);

                            Console.WriteLine("\nUpdating Roles for " + u.Username + "#" + u.Discriminator);
                            List<string> allUserRoles = new List<string>(); // All of the rolls that need to be assigned to the user

                            // Gets all roles that need to be assigned to the user in addition to removing those that interfere with roleGroups.json
                            allUserRoles = await SheetsFunctionality.GetRoles(row, u);

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
                                formattedRoles.Add(role);
                            }
                            // Add user to list of roles to redo
                            if (redo) redoUsers.Add(u, row);

                            // Add Roles To User
                            await SheetsFunctionality.AddRolesToUser(u, formattedRoles.ToArray());

                            // Find and set nickname
                            await SheetsFunctionality.FindAndSetNickname(u, row);
                            //Console.WriteLine("DONE");
                        }

                        await AssignNewRoles(g, redoUsers);
                    }
                }

            }
        }

        public static async Task AssignNewRoles(SocketGuild guild, Dictionary<SocketGuildUser,IList<object>> users)
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
