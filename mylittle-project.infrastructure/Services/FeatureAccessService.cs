using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.Infrastructure.Services
{
    public class FeatureAccessService : IFeatureAccessService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeatureAccessService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> IsFeatureEnabledAsync(Guid tenantId, string featureKey)
        {
            var query = _unitOfWork.TenantFeatures
                .Find(tf => tf.TenantId == tenantId && tf.IsEnabled)
                .Include(tf => tf.Feature);

            return await query.AnyAsync(tf => tf.Feature.Key == featureKey);
        }
    }
}
