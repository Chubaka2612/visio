using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Visio.Recognition.Services;
using Visio.Domain.Common;
using Visio.Services.ImageService;
using Visio.Data.Domain.Images;
using Microsoft.Azure.Cosmos;
using Visio.Data.Core.Db;

namespace Visio.Recognition
{
    public class VisionRecognitionFunction
    {
        private static readonly string AzureBlobSharedAccessToken = Environment.GetEnvironmentVariable("AzureBlobStorageOptions.SharedAccessToken");
        private static readonly string CompureVisionApiKeyServiceClientCredentials = Environment.GetEnvironmentVariable("CompureVision.ApiKeyServiceClientCredentials");
        private static readonly string CompureVisionEndpoint = Environment.GetEnvironmentVariable("CompureVision.Endpoint");

        private static readonly string AzureCosmosConnectionString = Environment.GetEnvironmentVariable("AzureCosmosOptions.ConnectionString");
        private static readonly string AzureCosmosDatabaseId = Environment.GetEnvironmentVariable("AzureCosmosOptions.DatabaseId");

        [FunctionName(nameof(VisionRecognitionFunction))]
        public async Task Run([ServiceBusTrigger("%ServiceBusTopic%", "%ServiceBusSubscription%", Connection = "AzureServiceBusConnection")] ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            ILogger log)
        {
            var imageRecognitionService = new ImageRecognitionService(
                new ComputerVisionClient(new ApiKeyServiceClientCredentials(CompureVisionApiKeyServiceClientCredentials))
                {
                    Endpoint = CompureVisionEndpoint
                });

            var imageService = new ImageService(new ImageDbRepository(new RepositoryOptions
            {
                CosmosClient = new CosmosClient(AzureCosmosConnectionString),
                DatabaseId = AzureCosmosDatabaseId
            }));
            try
            {
                var requestData = JsonConvert.DeserializeObject<Notification>(message.Body.ToString());
                if (requestData == null || string.IsNullOrEmpty(requestData.Content.ObjectPath))
                {
                    log.LogError("Invalid message data");
                    return;
                }

                log.LogInformation("Processing image: {ImageUrl}", requestData.Content.ObjectPath);

                // Manually renew lock while processing
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        await messageActions.RenewMessageLockAsync(message);
                        log.LogInformation("Message lock renewed.");
                    }
                });

                // Call Image Recognition Service
                List<string> tags = await imageRecognitionService.RecognizeImageAsync($"{requestData.Content.ObjectPath}?{AzureBlobSharedAccessToken}");

                log.LogInformation("Image recognized: {Description}", string.Join(",", tags));

                await imageService.UpdateImageLabelsAsync(requestData.Content.Id, tags);

                await messageActions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing image recognition");
            }
        }
    }
}