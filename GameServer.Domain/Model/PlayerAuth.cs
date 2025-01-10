using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Model
{
    public class PlayerAuth
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime AuthExpiration {  get; set; }

    }
}
