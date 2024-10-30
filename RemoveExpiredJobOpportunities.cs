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
            var response = await graphClient.Sites[Globals.siteId].Lists[Globals.listId].Items.GetAsync();
            var jobOpportunityListItems = response.Value;

            foreach (var item in jobOpportunityListItems)
            {
                var jobOpListItem = await graphClient
                .Sites[Globals.siteId]
                .Lists[Globals.listId]
                .Items[item.Id]
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Expand = ["fields"];
                });

                if (jobOpListItem != null)
                {
                    if (jobOpListItem.Fields.AdditionalData.TryGetValue("ApplicationDeadlineDate", out var deadlineDateObj) && deadlineDateObj is DateTime deadlineDate)
                    {
                        if (deadlineDate < DateTime.Now)
                        {
                            // Delete from list.
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"ListItemId: {jobOpListItem.Id} - The 'ApplicationDeadlineDate' field was not found or is not a valid date.");
                    }
                }
            }

            _logger.LogInformation("Backup completed successfully.");
        }
    }
}
