using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionAppInsights
{
   public interface IUserAppInsightsDataService
    {
        Task<int> GetActiveUsersCount(Guid schoolId, string userType);
    }
}
