using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Model
{
    public class Resource
    {
        [Key]
        public Guid Id { get; set; }
        public ResourceType Type { get; set; }
        public Guid PlayerId { get; set; }
        public int Amount { get; set; }
        public string UpdateWithAmount (int amount)
        {
            if (Amount + amount < 0)
            {
                return ("Insufficient funds");
            }

            Amount += amount;

            return Amount.ToString();
        }
    }

    public class ResourceTransaction
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey(nameof(Resource))]
        public Guid ResourceId { get; set; }
        public ResourceType Type { get; set; }
        public int Amount { get; set; }
    }
}
