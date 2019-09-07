using Discord.Commands;
using System.Threading.Tasks;


namespace DiscordBot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("update")] // forces an update to the roles on all connected servers
        public async Task Update()
        {
            await Sheets.UpdateRoles(Context.Client);
        }
    }
}
