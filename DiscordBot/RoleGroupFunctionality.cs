using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class RoleGroupFunctionality
    {

        public static async Task MatchRoleGroups(SocketGuildUser user, string role, string actualCell)
        {/*
            if (!Config.Bot.UseRoleGroups) return;
            try
            {
                foreach (RoleGroup group in Config.roleGroup.Groups)
                {
                    foreach (SocketRole userRole in user.Roles) // Checks all roles assigned to the user
                    {
                        if (group.roles.Contains(userRole.Name + ",") && group.roles.Contains(role + ",") && role != userRole.Name) // Checks for overlapping roles (from roleGroups.json)
                        {
                            // Removes the user's current role
                            if (!actualCell.Contains(userRole.Name) || !actualCell.Contains(role))
                            {
                                Console.WriteLine("ROLE GROUP MATCH: " + userRole.Name + ", " + role);
                                await user.RemoveRoleAsync(userRole);
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("\nroleGroups.json is either not set-up or incorrectly configured. Consider changing 'UseRoleGroups' to 'false' in config.json");
            }*/
        }
    }
}
