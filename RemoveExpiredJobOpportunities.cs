using Azure.Storage.Blobs;
using Azure.Storage.Queues;
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

            if (response != null && response.Value != null)
            {
                var jobOpportunityListItems = response.Value;
                var ids = new List<string>();

                foreach (var item in jobOpportunityListItems)
                {
                    if (item.Fields.AdditionalData.TryGetValue("ApplicationDeadlineDate", out var deadlineDateObj) && deadlineDateObj is DateTime deadlineDate)
                    {
                        if (deadlineDate.ToUniversalTime() < DateTime.UtcNow.AddMonths(-6))
                            ids.Add(item.Id);
                    }
                    else
                        _logger.LogWarning($"ListItemId: {item.Id} - The 'ApplicationDeadlineDate' field was not found or is not a valid date.");
                }

                if (ids.Any())
                {
                    foreach (var id in ids)
                    {
                        await SendMessageAsync(new DeleteMessage(id), _logger);
                    }
                }
            }
            else
            {
                _logger.LogError("Couldn't process request, enexpected response.");
            }

            

            _logger.LogInformation("RemoveExpiredJobOpportunities complete.");
        }

        private async Task SendMessageAsync(DeleteMessage payload, ILogger _logger)
        {
            var client = new QueueClient(Globals.azureWebJobsStorage, "delete");
            await client.CreateIfNotExistsAsync();

            if (client.Exists())
            {
                string message = JsonSerializer.Serialize(payload);
                await client.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
                _logger.LogInformation($"Sent to delete queue: {message}");
            }
        }

        internal class DeleteMessage
        {
            public DeleteMessage(string ids)
            {
                this.Ids = ids;
            }
            public string Ids { get; set; }
        }
    }
}
