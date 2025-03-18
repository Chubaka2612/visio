using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Visio.Recognition.Services;
using Visio.Domain.Common;


namespace Visio.Recognition
{
    public class VisionRecognitionFunction
    {
        private static readonly string AzureBlobSharedAccessToken = Environment.GetEnvironmentVariable("AzureBlobStorageOptions.SharedAccessToken");
        private static readonly string CompureVisionApiKeyServiceClientCredentials = Environment.GetEnvironmentVariable("AzureBlobStorageOptions.SharedAccessToken");
        private static readonly string CompureVisionEndpoint = Environment.GetEnvironmentVariable("AzureBlobStorageOptions.SharedAccessToken");

        [FunctionName(nameof(VisionRecognitionFunction))]
        public async Task Run([ServiceBusTrigger("visiotesttopic", "visiotestsbs", Connection = "AzureServiceBusConnection")] ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            ILogger log)
        {
            //TODO: DI of imageRecognitionService, imageService
            //Investigate DI + Functions Net8 
            var computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(CompureVisionApiKeyServiceClientCredentials))
            {
                Endpoint = CompureVisionApiKeyServiceClientCredentials
            };
            var imageRecognitionService = new ImageRecognitionService(computerVisionClient);

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

                //TODO: Update via ImageRepository item in CosmosDb

                await messageActions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing image recognition");
            }
        }
    }
}
