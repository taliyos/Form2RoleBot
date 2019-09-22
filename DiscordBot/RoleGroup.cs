using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class RoleGroup
    {
        public int columnNumber { get; set; }
        public List<string> roles { get; set; }

        public RoleGroup(int _columnNumber, List<string> _roles) {
            columnNumber = _columnNumber;
            roles = _roles;
        }
    }
}
