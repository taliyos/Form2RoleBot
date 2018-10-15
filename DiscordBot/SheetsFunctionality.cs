using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot
{
    class SheetsFunctionality
    {
        public static bool FindUsername(SocketGuildUser user, IList<object> userCell) // Checks if the username from the Google Sheets matches a discord user
        {
            string username = "NaN";


            if (Config.GoogleData.DiscordIDField != -1)
            {
                username = userCell[Config.GoogleData.DiscordIDField].ToString();
                username = username.Trim(); // trims excess characters
                username = username.Replace(" ", "");
                if (username != user.Username + "#" + user.Discriminator &&
                    username != user.Discriminator &&
                    username != user.Nickname + "#" +
                    user.Discriminator // Nickname is here just in case, but it is probably one of the worst ways of doing this since it'll change once the nickname userpdates
                ) return false;
            }
            else
            {
                username = userCell[0].ToString();
                username = username.Trim();
                username = username.Replace(" ", "");
                if (username != user.Username + "#" + user.Discriminator &&
                    username != user.Discriminator &&
                    username != user.Nickname + "#" + user.Discriminator
                ) return false;
            }

            return true;
        }


        public static string[] SeperateRoles(string role)
        {
            if (role.Contains(","))
            {
                role = role.Replace(" ", "");
                return role.Split(',');
            }
            else if (role.Contains("+"))
            {
                role = role.Replace(" ", "");
                return role.Split('+');
            }

            string[] seperatedRole = new string[1];
            seperatedRole[0] = role;
            return seperatedRole;
        }

        public static async Task CheckAndCreateRole(SocketGuild guild, string role)
        {
            bool roleFound = false;
            foreach (SocketRole dRole in guild.Roles)
            {
                if (dRole.Name.Equals(role))
                {
                    roleFound = true;
                    continue;
                }
            }
            if (!roleFound)
            {
                await guild.CreateRoleAsync(role);
            }
        }

        public static async Task<SocketRole> CreateRole(SocketGuild guild, string role)
        {
            await guild.CreateRoleAsync(role);
            return guild.Roles.FirstOrDefault(x => x.Name == role);
        }

        public static async Task MatchRoleGroups(SocketGuildUser user, string role)
        {
            foreach (string roleGroup in Config.RoleGroup.Groups)
            {
                foreach (SocketRole userRole in user.Roles) // Checks all roles assigned to the user
                {
                    if (roleGroup.Contains(userRole.Name) && roleGroup.Contains(role)) // Checks for overlapping roles (from roleGroups.json)
                    {
                        // Removes the user's current role
                        await user.RemoveRoleAsync(userRole);
                    }
                }
            }
        }

        public static async Task<List<string>> GetRoles(IList<object> userData, SocketGuildUser user)
        {
            List<string> allUserRoles = new List<string>();
            List<string> finalizedUserRoles = new List<string>();
            SocketRole[] assignedRoles = user.Roles.ToArray();
            for (int i = Config.GoogleData.RolesStartAfter; i < userData.Count - Config.GoogleData.RolesEndBefore; i++)
            {
                string roleName = userData[i].ToString();

                // Goto the next cell if there's no role
                if (roleName.Equals("None") || roleName.Equals("")) continue;


                //Seperates roles into an array
                string[] seperatedRoles = SeperateRoles(roleName);

                foreach (string formRole in seperatedRoles)
                {
                    //await SheetsFunctionality.CheckAndCreateRole(g, formRole); // A new role is created if it doesn't exist (This is now done when formatting roles)
                    //Console.WriteLine("Role removed " + formRole);
                    await MatchRoleGroups(user, formRole); // Removes roles that interfere with each other as defined in the roleGroups.json configuration file
                }


                allUserRoles.AddRange(seperatedRoles);
            }
            //finalizedUserRoles = allUserRoles;
            foreach (string role in allUserRoles)
            {
                finalizedUserRoles.Add(role);
            }
            /*foreach (SocketRole assignedRole in assignedRoles)
            {
                foreach (string userRole in allUserRoles)
                {
                    if (userRole == assignedRole.Name)
                    {
                        Console.WriteLine("REMOVING " + userRole);
                        finalizedUserRoles.Remove(userRole);
                    }
                }
            }*/
            return finalizedUserRoles;
            //return allUserRoles;
        }


        public static async Task FindAndSetNickname(SocketGuildUser user, IList<object> userCell)
        {
            string nickname;

            if (Config.GoogleData.NicknameField == -2) // finds the nickname in the Google Sheets data
                return;

            if (Config.GoogleData.NicknameField != -1)
                nickname = userCell[Config.GoogleData.NicknameField].ToString();
            else
                nickname = userCell[userCell.Count - 1].ToString();

            await SetNickname(user, nickname);
        }

        public static async Task SetNickname(SocketGuildUser user, string nickname) // sets the user's nickname
        {
            try
            {
                // sets nickname
                await user.ModifyAsync(x =>
                {
                    x.Nickname = nickname;
                });
            }
            catch
            {
                // occurs when the user is ranked above the bot
                Console.WriteLine("No nickname was specified or their rank is too high.");
            }
        }

        public static async Task AddRolesToUser(SocketGuildUser user, SocketRole[] roles)
        {
            List<SocketRole> updatedRoles = roles.ToList();
            foreach (SocketRole role in roles) // gets rid of roles the user already has to help prevent discord limits
            {
                if (user.Roles.Contains(role))
                {
                    updatedRoles.Remove(role);
                }
            }
            await user.AddRolesAsync(updatedRoles);
        }

        public static async Task StoreUserID(SocketGuildUser user)
        {
            Config.AppendToIDs(user.Id.ToString());
        }
    }
}
