using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionAppInsights
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            IUserAppInsightsDataService IUserAppInsightsDataService = new UserAppInsightsDataService();
            Guid gd = new Guid("ad8c2f3e-72b9-4390-bfb1-09bb3619b9df");
            await IUserAppInsightsDataService.GetActiveUsersCount(gd, "ALLPARENTS");
            return  req.CreateResponse(HttpStatusCode.Accepted, "check on the browser");
        }
    }
}
