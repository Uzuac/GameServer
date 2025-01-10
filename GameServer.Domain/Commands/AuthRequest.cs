using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Commands
{
    public class AuthRequest
    {
        public Guid DeviceId { get; set; }
    }
}
