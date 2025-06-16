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


            if (response != null && response.Value != null)
            {
                var jobOpportunityListItems = response.Value;

                foreach (var listItem in jobOpportunityListItems)
                {
                    if (listItem.Fields != null)
                    {
                        // unescape unicode characters
                        var json = Regex.Unescape(JsonSerializer.Serialize(listItem.Fields.AdditionalData));
                        // replace double quotation marks with single quotation marks
                        json = json.Replace("\"\"", "\"");
                        // escape single quotation marks
                        json = json.Replace("\"", "\\\"");

                        backup.JobOpportunities.Add(json);
                    }
                }

                if (backup.JobOpportunities.Count > 0)
                {
                    // indent the json file to make it easier to read
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    // unescape unicode characters
                    var json = Regex.Unescape(JsonSerializer.Serialize(backup, options));

                    try
                    {
                        var byteArray = Encoding.UTF8.GetBytes(json);
                        string blobName = $"{backup.CreateDate.ToString("yyyy-MM-dd")}-jobOpportunities.json";

                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Globals.containerName);
                        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
                        var blobClient = containerClient.GetBlobClient(blobName);

                        using var stream = new MemoryStream(byteArray);
                        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "application/json" });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error saving blob file!");
                        _logger.LogError($"{ex.Message}");
                        _logger.LogError($"{ex.StackTrace}");
                    }
                }
            }

            _logger.LogInformation($"Backed up {backup.JobOpportunities.Count} jobs.{Environment.NewLine}Backup complete!");
        }
    }
}