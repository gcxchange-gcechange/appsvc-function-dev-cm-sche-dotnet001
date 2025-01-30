using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Text;
using System.Text.Json;

namespace appsvc_function_dev_cm_sche_dotnet001
{
    public class BackupJobOpportunities
    {
        private readonly ILogger _logger;

        public BackupJobOpportunities(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BackupJobOpportunities>();
        }

        [Function("BackupJobOpportunities")]
        public async Task RunAsync([TimerTrigger("0 0 5 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"BackupJobOpportunities function executed at: {DateTime.Now}");

            var backup = new Backup();
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

            foreach (var listItem in jobOpportunityListItems)
            {
                backup.JobOpportunities.Add(JsonSerializer.Serialize(listItem.Fields.AdditionalData));
            }

            if (backup.JobOpportunities.Any())
            {
                var byteArray = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(backup));
                using var stream = new MemoryStream(byteArray);

                string blobName = $"{backup.CreateDate.ToString("yyyy-MM-dd")}-jobOpportunities.json";
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Globals.containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "application/json" });
            }

            _logger.LogInformation($"Backed up {backup.JobOpportunities.Count} jobs.{Environment.NewLine}Backup complete!");
        }
    }
}
