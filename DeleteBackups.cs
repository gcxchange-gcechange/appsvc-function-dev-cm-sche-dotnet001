using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace appsvc_function_dev_cm_sche_dotnet001
{
    public  class DeleteBackups
    {
        private readonly ILogger _logger;

        public DeleteBackups(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DeleteBackups>();
        }

        [Function("DeleteBackups")]
        public async Task RunAsync([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"DeleteBackups function executed at: {DateTime.Now}");

            var blobServiceClient = new BlobServiceClient(Globals.azureWebJobsStorage);
            var containerClient = blobServiceClient.GetBlobContainerClient(Globals.containerName);

            var thresholdDate = DateTime.UtcNow.AddMonths(-6);
            var regex = new Regex(@"(\d{4})-(\d{2})-(\d{2})-jobOpportunities\.json");

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var match = regex.Match(blobItem.Name);

                if (match.Success)
                {
                    int year = int.Parse(match.Groups[1].Value);
                    int month = int.Parse(match.Groups[2].Value);
                    int day = int.Parse(match.Groups[3].Value);
                    DateTime blobDate = new DateTime(year, month, day);

                    if (blobDate < thresholdDate)
                    {
                        var blobClient = containerClient.GetBlobClient(blobItem.Name);
                        await blobClient.DeleteIfExistsAsync();
                        _logger.LogInformation($"Deleted blob: {blobItem.Name}");
                    }
                }
            }

            _logger.LogInformation($"DeleteBackups completed.");
        }
    }
}
