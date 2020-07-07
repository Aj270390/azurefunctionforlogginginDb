using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionAppInsights
{
    internal class UserAppInsightsDataService : IUserAppInsightsDataService
    {
        private const string AllStudentsGroupId = "ALLSTUDENTS";
        private const string AllParentsGroupId = "ALLPARENTS";
        public async Task<int> GetActiveUsersCount(Guid schoolId, string userType)
        {
            var apiId = GetApiId(userType);
            var apiKey = GetApiKey(userType);
            var appInsightsUrl = Environment.GetEnvironmentVariable("app-insights-endpoint");
            var storageConnectionString = Environment.GetEnvironmentVariable("capita.product-analytics.appinsights.storage-account-connection-string");

            try
            {
                List<AppInsightsCustomEventEntity> listAppInsightsCustomEventData = new List<AppInsightsCustomEventEntity>();
                int userCount = 0;
                HttpResponseMessage response;
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), $"{appInsightsUrl}{apiId}/query?timespan=P30D"))
                    {
                        request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
                        request.Content = new StringContent("{     \"query\": \"customEvents | where timestamp >= ago(90d)" +
                                                                            "| summarize  Users = dcount(user_Id), "+
                                                                            "Count = count(user_AuthenticatedId) by recordedon = format_datetime(timestamp, 'yy-MM-dd'), EventName = name, IsApp = tobool(tostring(customDimensions.IsApp))"+
                                                                            "| order by recordedon desc\" }", Encoding.UTF8, "application/json");
                        response = await httpClient.SendAsync(request);
                    }
                }
                var results = response.Content.ReadAsStringAsync().Result;
                var data = (JObject)JsonConvert.DeserializeObject(results);

                if (data["tables"] != null && data["tables"].Any())
                {
                    if (data["tables"][0]["rows"] != null && data["tables"][0]["rows"].Any())
                    {
                        for (int i = 0; i < data["tables"][0]["rows"].Count(); i++)
                        {
                            AppInsightsCustomEventEntity appInsightsCustomEventData = new AppInsightsCustomEventEntity();
                            appInsightsCustomEventData.EventName= Convert.ToString(data["tables"][0]["rows"][i][0]);
                            appInsightsCustomEventData.Users = Convert.ToDouble(data["tables"][0]["rows"][i][1]);
                            appInsightsCustomEventData.Sessions = Convert.ToDouble(data["tables"][0]["rows"][i][2]);
                            appInsightsCustomEventData.Count = Convert.ToDouble(data["tables"][0]["rows"][i][3]);
                            listAppInsightsCustomEventData.Add(appInsightsCustomEventData);
                        }
                    }
                }
                var dataToDump = JsonConvert.SerializeObject(listAppInsightsCustomEventData);

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
                string tableName = "appinsightsloggingAj";
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                CreateNewTable(cloudTable);
                InsertRecordToTable(cloudTable, listAppInsightsCustomEventData);
                return userCount;
            }
            catch (Exception ex)
            {

                return -1;
            }
        }
        public static void InsertRecordToTable(CloudTable table, List<AppInsightsCustomEventEntity> listAppInsightsCustomEventData)
        {
            
            if (listAppInsightsCustomEventData.Count()>0)
            {
                foreach(var entity in listAppInsightsCustomEventData)
                {
                    entity.AssignPartitionKey();
                    entity.AssignRowKey();
                    TableOperation tableOperation = TableOperation.Insert(entity);
                    table.Execute(tableOperation);
                    Console.WriteLine("Record inserted");
                }
            }
        }

        private static void CreateNewTable(CloudTable table)
        {
            if (!table.CreateIfNotExists())
            {
                Console.WriteLine("Table {0} already exists", table.Name);
                return;
            }
            Console.WriteLine("Table {0} created", table.Name);
        }

        private string GetApiId(string userType)
        {
            switch (userType)
            {
                case AllStudentsGroupId:
                    return Environment.GetEnvironmentVariable("sims.school-student.insights.setting.appinsights-id");
                case AllParentsGroupId:
                    return Environment.GetEnvironmentVariable("sims.school-parent.insights.setting.appinsights-id");
                default:
                    return "";
            }
        }

        private string GetApiKey(string userType)
        {
            switch (userType)
            {
                case AllStudentsGroupId:
                    return Environment.GetEnvironmentVariable("sims.school-student.insights.setting.appinsights-key");
                case AllParentsGroupId:
                    return Environment.GetEnvironmentVariable("sims.school-parent.insights.setting.appinsights-key");
                default:
                    return "";
            }
        }
    }
}
