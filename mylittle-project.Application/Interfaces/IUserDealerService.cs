using mylittle_project.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IUserDealerService
    {
        // ✅ Create a new user under an existing Business (dealer is already created via BusinessInfo)
        Task<Guid> AddUserAsync(UserDealerDto dto);

        // ✅ Get all users across all businesses (Admin View)
        Task<List<UserDealerDto>> GetAllUsersAsync();

        // ✅ Get all users under a specific business
        Task<List<UserDealerDto>> GetUsersByDealerAsync(Guid businessInfoId);
    }
}
