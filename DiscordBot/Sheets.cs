using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace DiscordBot
{
    class Sheets
    {

        private string sheetID = Config.GoogleData.SpreadsheetID;
        private string range = Config.GoogleData.Range;

        private SheetsService service;

        private IList<IList<Object>> previousSheetValues;

        public async Task UpdateRoles(DiscordSocketClient client)
        {
            service = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = Config.GoogleData.APIKey,
                ApplicationName = "Form2Role Bot"
            });

            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(sheetID, range);

            ValueRange responses = request.Execute();
            previousSheetValues = responses.Values;

            

           if (previousSheetValues != null && previousSheetValues.Count > 0)
            {
                foreach (SocketGuild g in client.Guilds)
                {
                    Console.WriteLine("\n\nUpdating roles in " + g.Name + ".");
                    foreach (SocketGuildUser u in g.Users)
                    {
                        foreach (var row in previousSheetValues)
                        {
                            if (row[0].ToString() == u.Username + "#" + u.Discriminator ||
                                row[0].ToString() == u.Discriminator ||
                                row[0].ToString() == u.Nickname + "#" + u.Discriminator)
                            { // Checks for matching user
                                Console.WriteLine("\nUpdating Roles for " + u.Username + "#" + u.Discriminator);
                                List<string> roles = new List<string>();
                                for (int i = Config.GoogleData.RolesStartAfter; i < row.Count - 1; i++) // Loops through each role and which will be added to the user
                                {
                                    string roleName = row[i].ToString();
                                    if (!roleName.Equals("None") && !roleName.Equals(""))
                                    {
                                        string[] seperatedRoles = new string[1];
                                        seperatedRoles[0] = roleName;

                                        if (roleName.Contains(","))
                                        {
                                            roleName = roleName.Replace(" ", "");
                                            seperatedRoles = roleName.Split(',');
                                        } else if (roleName.Contains("+"))
                                        {
                                            roleName = roleName.Replace(" ", "");
                                            seperatedRoles = roleName.Split('+');
                                        }

                                        foreach (string s in seperatedRoles)
                                        {
                                            roles.Add(s);
                                        }
                                    }
                                }

                                List<SocketRole> formattedRoles = new List<SocketRole>();
                                foreach (string s in roles)
                                {
                                    formattedRoles.Add(g.Roles.FirstOrDefault(x => x.Name == s)); // adds the first found role matching the name or none at all
                                }

                                try
                                {
                                    IGuildUser gUser = u as IGuildUser;
                                    await gUser.AddRolesAsync(formattedRoles.ToArray());
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("An error occurred while assigning roles: " + e);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CheckSheets()
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(sheetID, range);

            ValueRange responses = request.Execute();
            IList<IList<Object>> values = responses.Values;

            if (values != null && values.Count > 0)
            {

            }
        }

    }
}
