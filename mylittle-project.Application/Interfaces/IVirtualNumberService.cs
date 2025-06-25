using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IVirtualNumberService
    {
        Task<string> AssignVirtualNumberAsync(Guid businessInfoId);
        Task<string> GetAssignedNumberAsync(Guid businessInfoId);
    }
}
