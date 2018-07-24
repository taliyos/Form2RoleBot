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

        private static readonly string SheetId = Config.GoogleData.SpreadsheetID;
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
                            if (Config.GoogleData.DiscordIDField != -1)
                            {
                                if (row[Config.GoogleData.DiscordIDField].ToString() != u.Username + "#" + u.Discriminator &&
                                    row[Config.GoogleData.DiscordIDField].ToString() != u.Discriminator &&
                                    row[Config.GoogleData.DiscordIDField].ToString() != u.Nickname + "#" + u.Discriminator // Nickname is here just in case, but it is probably one of the worst ways of doing this since it'll change once the nickname updates
                                ) continue; // Checks for matching user
                            }
                            else
                            {
                                if (row[0].ToString() != u.Username + "#" + u.Discriminator &&
                                    row[0].ToString() != u.Discriminator &&
                                    row[0].ToString() != u.Nickname + "#" + u.Discriminator
                                ) continue; // Checks for matching user
                            }
                            

                            Console.WriteLine("\nUpdating Roles for " + u.Username + "#" + u.Discriminator);
                            List<string> roles = new List<string>();

                            for (int i = Config.GoogleData.RolesStartAfter; i < row.Count - 1 - Config.GoogleData.RolesEndBefore; i++) // Loops through each role and which will be added to the user
                            {
                                string roleName = row[i].ToString();

                                if (roleName.Equals("None") || roleName.Equals("")) continue;

                                string[] seperatedRoles = new string[1];
                                seperatedRoles[0] = roleName;

                                if (roleName.Contains(","))
                                {
                                    roleName = roleName.Replace(" ", "");
                                    seperatedRoles = roleName.Split(',');
                                }
                                else if (roleName.Contains("+"))
                                {
                                    roleName = roleName.Replace(" ", "");
                                    seperatedRoles = roleName.Split('+');
                                }

                                roles.AddRange(seperatedRoles);
                            }


                            List<SocketRole> formattedRoles = new List<SocketRole>();

                            // Roles
                            foreach (string s in roles)
                            {
                                formattedRoles.Add(g.Roles.FirstOrDefault(x => x.Name == s)); // adds the first found role matching the name or none at all
                            }

                                //Add Roles To User
                            try
                            {
                                await u.AddRolesAsync(formattedRoles.ToArray());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("An error occurred while assigning roles: " + e);
                            }

                            // Change Nickname
                            if (Config.GoogleData.NicknameField == -2)
                            {
                                // No Nickname field
                                return;
                            }
                            if (Config.GoogleData.NicknameField != -1)
                            {
                                try
                                {
                                    await u.ModifyAsync(x =>
                                    {
                                        x.Nickname = row[Config.GoogleData.NicknameField].ToString();
                                    });
                                }
                                catch
                                {
                                    Console.WriteLine("No nickname specified or their rank is too high.");
                                }
                            }

                            else
                            {
                                try
                                {
                                    await u.ModifyAsync(x =>
                                    {
                                        x.Nickname = row[row.Count-1].ToString();
                                    });
                                }
                                catch
                                {
                                    Console.WriteLine("No nickname specified or their rank is too high.");
                                }
                            }
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

            // Checking if the newly retrieved sheet is the same as the previous one
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
            }

            Console.WriteLine("[" + DateTime.Now + "] No update from linked Google Sheet.");

        }

    }
}
