using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Color = Discord.Color;


namespace DiscordBot.Modules
{


    public class Misc : ModuleBase<SocketCommandContext>
    {
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Form2Role Bot - Google Sheets API";

        [Command("update")]
        public async Task Update()
        {
            // Create Google Sheets API service.
            SheetsService service = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = Config.GoogleData.APIKey,
                ApplicationName = ApplicationName
            });

            // Define request parameters.
            string spreadsheetId = Config.GoogleData.SpreadsheetID;
            string range = Config.GoogleData.Range;

            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);


            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var u in Context.Guild.Users)
                {
                    Console.WriteLine(u.Username + "#"+ u.Discriminator);
                    foreach (var row in values)
                    {
                        if (row[0].ToString() == u.Username + "#" + u.Discriminator || row[0].ToString() == u.Discriminator || row[0].ToString() == u.Nickname + "#" + u.Discriminator) // Included just the dicriminator for compatability
                        {
                            Console.WriteLine("\n\nMatch Found!");
                            Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}", row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10]);
                            for (int i = 1; i < row.Count-1; i++)
                            {
                                string roleName = row[i].ToString();
                                if (roleName != "None" && roleName != "")
                                {
                                    string[] roles = new string[1];
                                    roles[0] = roleName;
                                    if (roleName.Contains(','))
                                    {
                                        roleName = roleName.Replace(" ", "");
                                        roles = roleName.Split(',');
                                    }

                                    foreach (string r in roles)
                                    {
                                        Console.WriteLine("\nAssigning role: {0}", r);
                                        SocketRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == r);
                                        try
                                        {
                                            IGuildUser gUser = u as IGuildUser;
                                            await gUser.AddRoleAsync(role);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(r + " not found on server, skipping...");
                                        }
                                    }
                                }
                            }

                            try
                            {
                                await u.ModifyAsync(x => { x.Nickname = row[row.Count - 1].ToString(); });
                            }
                            catch (Exception e)
                            {

                            }

                        }
                    }
                }
                Console.WriteLine("Discord ID, Graduation Year, Major, Math, Physics, Chemistry, Biology, English, History, Lanague, Other");
            }
            else
            {
                Console.WriteLine("No data found.");
                string message =
                    "No data found. Please fill in the form at https://goo.gl/forms/4jKCse8WQZYZ6JSw2 first and try again.";
                await Context.Channel.SendMessageAsync(message);
            }


        }
    }
}
