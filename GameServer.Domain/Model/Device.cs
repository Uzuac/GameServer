using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Model
{
    public class Device
    {
        public Guid Id { get; set; }
        public Guid PlayerId {get; set; }

        public PlayerAuth PlayerAuth { get; set; }

        public bool IsAuthenticated()
        {
            return PlayerAuth?.AuthExpiration > DateTime.UtcNow;
        }
    }
}
