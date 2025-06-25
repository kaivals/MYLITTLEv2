using mylittle_project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(Guid id);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
    }

}
