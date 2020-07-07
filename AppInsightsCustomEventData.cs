using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionAppInsights
{
    class AppInsightsCustomEventEntity: TableEntity
    {
        public string EventName { get; set; }
        public double Users { get; set; }
        public double Sessions { get; set; }
        public double Count { get; set; }
        public void AssignRowKey()
        {
            this.RowKey = EventName.ToString();
        }
        public void AssignPartitionKey()
        {
            this.PartitionKey = Guid.NewGuid().ToString();
        }
    }
}
