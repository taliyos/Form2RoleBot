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
            string discordUsername = user.Username.ToLower();
            if (Config.GoogleData.DiscordIDField != -1)
            {
                username = userCell[Config.GoogleData.DiscordIDField].ToString();
                username = username.Trim().ToLower(); // trims excess characters

                if (username != discordUsername + "#" + user.Discriminator && username != user.Discriminator) return false;
            }
            else
            {
                username = userCell[0].ToString();
                username = username.Trim().ToLower();
                
                if (username != discordUsername + "#" + user.Discriminator && username != user.Discriminator) return false;
            }

            return true;
        }


        public static string[] SeperateRoles(string role)
        {
            if (role.Contains(","))
            {

                role = role.Trim();

                string[] roles = role.Split(',');
                for (int i = 0; i < roles.Length; i++)
                {
                    roles[i] = roles[i].Trim();

                }
                return roles;
            }
            else if (role.Contains("+"))
            {

                role = role.Trim();

                string[] roles = role.Split('+');
                for (int i = 0; i < roles.Length; i++)
                {
                    roles[i] = roles[i].Trim();

                }
                return roles;
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
            guild.CreateRoleAsync(role).Wait();
            SocketRole sRole = guild.Roles.FirstOrDefault(x => x.Name == role);
            return sRole;
        }


        public static async Task<List<string>> GetRoles(IList<object> userRow, SocketGuildUser user)
        {

            List<string> allUserRoles = new List<string>();
            SocketRole[] assignedRoles = user.Roles.ToArray();
            int columnNumber = 0;
            for (int i = Config.GoogleData.RolesStartAfter; i < userRow.Count - Config.GoogleData.RolesEndBefore; i++)
            {
                string roleName = userRow[i].ToString().Trim();

                // Goto the next cell if there's no role
                if (roleName.Equals("None") || roleName.Equals("")) {
                    await RoleGroupFunctionality.RemovePreviousRole(user, columnNumber);
                    continue;
                }

                //Seperates roles into an array
                string[] seperatedRoles = SeperateRoles(roleName);

                // Uses a foreach in case two or more roles are specified in one input
                await RoleGroupFunctionality.MatchRoleGroups(user, roleName, seperatedRoles, columnNumber); // Removes roles that interfere with each other as defined in the roleGroups.json configuration file

                allUserRoles.AddRange(seperatedRoles);
                columnNumber++;
            }

            return allUserRoles;
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
                Console.WriteLine("No nickname specified or their rank is too high.");
            }
        }

        public static async Task AddRolesToUser(SocketGuildUser user, SocketRole[] roles)
        {
            if (roles.Length == 0) return;
            List<SocketRole> updatedRoles = roles.ToList(); // List error if SocketRole[] roles is passed as a list
            foreach (SocketRole role in roles) // gets rid of roles the user already has to help prevent discord limits
            {
                if (user.Roles.Contains(role))
                {
                    updatedRoles.Remove(role);
                }
            }
            if (updatedRoles.Count == 0) return;
            await user.AddRolesAsync(updatedRoles);
        }

        public static async Task StoreUserID(SocketGuildUser user)
        {
            Config.AppendToIDs(user.Id.ToString());
        }
    }
}
