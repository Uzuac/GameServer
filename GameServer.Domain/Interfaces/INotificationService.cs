using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Interfaces
{
    public interface INotificationService
    {
        Task SendNotification(List<Guid> deviceIds, string message);
    }
}
