
using Azure.Messaging.ServiceBus;
using log4net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Visio.Services.Notifications
{
    public class NotificationProducer : INotificationProducer
    {
        private readonly ServiceBusClient _busClient;
        public  ServiceBusOptions Options { get; set; }
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificationProducer));

        public NotificationProducer(ServiceBusOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.ConnectionString);
            ArgumentNullException.ThrowIfNull(options.QueueName);

            Options = options;
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
                _logger.InfoFormat("A message was sent to the queue '{QueueName}'", queueName);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat(ex.Message, "An exception occurred when sending a message to the queue '{QueueName}'", queueName);
                throw;
            }
        }
    }
}
