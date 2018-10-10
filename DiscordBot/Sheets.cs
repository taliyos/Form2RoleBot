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
                    Console.WriteLine("\n\nUpdating roles in " + g.Name + ".");
                    foreach (SocketGuildUser u in g.Users)
                    {
                        foreach (var row in values)
                        {
                            if (!FindUsername(row, u)) continue;

                            Console.WriteLine("\nUpdating Roles for " + u.Username + "#" + u.Discriminator);
                            List<string> allUserRoles = new List<string>(); // All of the rolls that need to be assigned to the user

                            // Gets all rolls that need to be assigned to the user in addition to removing those that interfere with roleGroups.json
                            for (int i = Config.GoogleData.RolesStartAfter; i < row.Count - 1 - Config.GoogleData.RolesEndBefore; i++) 
                            {
                                string roleName = row[i].ToString();

                                // Go to the next cell if there's no role
                                if (roleName.Equals("None") || roleName.Equals("")) continue;


                                //Seperates roles into an array
                                string[] seperatedRoles = SheetsFunctionality.SeperateRoles(roleName);

                                foreach (string formRole in seperatedRoles)
                                {
                                    //await SheetsFunctionality.CheckAndCreateRole(g, formRole); // A new role is created if it doesn't exist (This is now done when formatting roles)

                                    await SheetsFunctionality.RemoveRole(u, formRole); // Removes roles that interfere with each other as defined in the roleGroups.json configuration file
                                }

                                allUserRoles.AddRange(seperatedRoles);
                            }

                            List<SocketRole> formattedRoles = new List<SocketRole>();

                            // Google Sheets data to SocketRole
                            foreach (string s in allUserRoles)
                            {
                                SocketRole role = g.Roles.FirstOrDefault(x => x.Name == s);
                                if (role == default(SocketRole))
                                {
                                    role = await SheetsFunctionality.CreateRole(g, s);
                                    Console.WriteLine("Creating Role...");
                                }
                                formattedRoles.Add(role);
                            }

                            // Add Roles To User
                            await u.AddRolesAsync(formattedRoles.ToArray());

                            // Find and set nickname
                            await SheetsFunctionality.FindAndSetNickname(u, row);

                        }
                    }
                }
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
