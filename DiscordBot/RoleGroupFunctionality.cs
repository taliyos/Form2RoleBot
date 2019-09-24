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

        public static async Task MatchRoleGroups(SocketGuildUser user, string formRole, string[] allRoles, int columnNumber)
        {
            if (!Config.Bot.UseRoleGroups) return;
            foreach (string actualCell in allRoles)
            {
                foreach (RoleGroup rGroup in Config.roleGroup)
                {
                    if (rGroup.columnNumber != columnNumber) continue;

                    foreach (SocketRole userRole in user.Roles) // Checks all roles assigned to the user
                    {
                        if (rGroup.roles.Contains(userRole.Name) && rGroup.roles.Contains(formRole) && formRole != userRole.Name) // Checks for overlapping roles (from roleGroups.json)
                        {
                            // Removes the user's current role
                            if (!formRole.Contains(userRole.Name) || !actualCell.Contains(formRole))
                            {
                                await user.RemoveRoleAsync(userRole);
                            }
                        }
                    }
                }
            }
        }

        public static async Task RemovePreviousRole(SocketGuildUser user, int columnNumber) {
            if (!Config.Bot.UseRoleGroups) return;
            foreach (SocketRole role in user.Roles) {
                foreach (RoleGroup rGroup in Config.roleGroup) {
                    if (columnNumber != rGroup.columnNumber) continue;
                    foreach (string configRole in rGroup.roles) {
                        if (role.Name.ToLower().Equals(configRole.ToLower())) {
                            await user.RemoveRoleAsync(role);
                            return;
                        }
                    }
                }
            }
        }
    }
}
