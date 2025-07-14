using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public abstract class BaseEntity : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
    }
}
