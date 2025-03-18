
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Visio.Services.Notifications
{
    public class NotificationProducer : INotificationProducer
    {
        private readonly ServiceBusClient _busClient;
        public  ServiceBusOptions Options { get; set; }

        private readonly ILogger<NotificationProducer> _logger;

        public NotificationProducer(ServiceBusOptions options, ILogger<NotificationProducer> logger)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.ConnectionString);
            ArgumentNullException.ThrowIfNull(options.QueueName);
            ArgumentNullException.ThrowIfNull(logger);

            Options = options;
            _logger = logger;
            _busClient = new ServiceBusClient(options.ConnectionString);
        }

        public async Task PublishMessageAsync<T>(T message, string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
            }

            try
            {
                ServiceBusSender sender = _busClient.CreateSender(queueName);

                var serializedMessage = JsonConvert.SerializeObject(message, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                var queueMessage = new ServiceBusMessage(new BinaryData(serializedMessage));

                await sender.SendMessageAsync(queueMessage);
                _logger.LogInformation("A message was sent to the queue '{QueueName}'", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred when sending a message to the queue '{QueueName}'", queueName);
                throw;
            }
        }
    }
}
