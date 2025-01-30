using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Text;
using System.Text.Json;

namespace appsvc_function_dev_cm_sche_dotnet001
{
    public class RemoveExpiredJobOpportunities
    {
        private readonly ILogger _logger;

        public RemoveExpiredJobOpportunities(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RemoveExpiredJobOpportunities>();
        }

        [Function("RemoveExpiredJobOpportunities")]
        public async Task RunAsync([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"RemoveExpiredJobOpportunities function executed at: {DateTime.Now}");

            var blobServiceClient = new BlobServiceClient(Globals.azureWebJobsStorage);
            var graphClient = new GraphServiceClient(new ROPCConfidentialTokenCredential(_logger));

            var response = await graphClient
            .Sites[Globals.siteId]
            .Lists[Globals.listId]
            .Items
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Expand = ["fields"];
            });

            var jobOpportunityListItems = response.Value;
            var itemIds = new List<string>();

            foreach (var item in jobOpportunityListItems)
            {
                if (item.Fields.AdditionalData.TryGetValue("ApplicationDeadlineDate", out var deadlineDateObj) && deadlineDateObj is DateTime deadlineDate)
                {
                    if (deadlineDate.ToUniversalTime() < DateTime.UtcNow.AddMonths(-1))
                        itemIds.Add(item.Id);
                }
                else
                    _logger.LogWarning($"ListItemId: {item.Id} - The 'ApplicationDeadlineDate' field was not found or is not a valid date.");
            }

            if (itemIds.Any())
            {
                var data = new
                {
                    ItemId = string.Join(",", itemIds)
                };

                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                var httpClient = new HttpClient();
                var responseContent = await httpClient.PostAsync(Globals.deleteFunctionUrl, content);

                if (responseContent.IsSuccessStatusCode)
                    _logger.LogInformation($"Successfully deleted {itemIds.Count} expired job opportunities.");
                else
                    _logger.LogError($"Something went wrong: {responseContent.StatusCode} - {responseContent.Content}");
            }

            _logger.LogInformation("RemoveExpiredJobOpportunities complete.");
        }
    }
}
