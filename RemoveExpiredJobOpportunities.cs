using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using CareerMarketplace;

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

            foreach (var item in jobOpportunityListItems)
            {
                if (item.Fields.AdditionalData.TryGetValue("ApplicationDeadlineDate", out var deadlineDateObj) && deadlineDateObj is DateTime deadlineDate)
                {
                    if (deadlineDate < DateTime.Now)
                    {
                        await graphClient
                        .Sites[Globals.siteId]
                        .Lists[Globals.listId]
                        .Items[item.Id]
                        .DeleteAsync();

                        _logger.LogInformation($"Deleted expired JobOpportunity with ListItemId: {item.Id}");
                    }
                }
                else
                {
                    _logger.LogWarning($"ListItemId: {item.Id} - The 'ApplicationDeadlineDate' field was not found or is not a valid date.");
                }
            }

            _logger.LogInformation("Backup completed successfully.");
        }
    }
}
