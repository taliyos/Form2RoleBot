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
                            List<string> roles = new List<string>();

                            for (int i = Config.GoogleData.RolesStartAfter; i < row.Count - 1 - Config.GoogleData.RolesEndBefore; i++) // Loops through each role and which will be added to the user
                            {
                                string roleName = row[i].ToString();

                                // Go to next cell if there's no role
                                if (roleName.Equals("None") || roleName.Equals("")) continue;


                                //Seperating Roles
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

                                foreach (string formRole in seperatedRoles)
                                {
                                    foreach (string roleGroup in Config.RoleGroup.Groups)
                                    {
                                        foreach (SocketRole userRole in u.Roles)
                                        {
                                            if (roleGroup.Contains(userRole.Name) && roleGroup.Contains(formRole))
                                            {
                                                // Get rid of User role to add form Role
                                                await u.RemoveRoleAsync(userRole);
                                            }
                                        }
                                    }
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

        private static bool FindUsername(IList<object> row, SocketGuildUser u) // Checks if the username from the Google Sheets matches a discord user
        {
            string username = "NaN";
            if (Config.GoogleData.DiscordIDField != -1)
            {
                username = row[Config.GoogleData.DiscordIDField].ToString();
                username = username.Trim(); // trims excess characters
                username = username.Replace(" ", "");
                if (username != u.Username + "#" + u.Discriminator &&
                    username != u.Discriminator &&
                    username != u.Nickname + "#" +
                    u.Discriminator // Nickname is here just in case, but it is probably one of the worst ways of doing this since it'll change once the nickname updates
                ) return false;
            }
            else
            {
                username = row[0].ToString();
                username = username.Trim();
                username = username.Replace(" ", "");
                if (username != u.Username + "#" + u.Discriminator &&
                    username != u.Discriminator &&
                    username != u.Nickname + "#" + u.Discriminator
                ) return false;
            }

            return true;
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
