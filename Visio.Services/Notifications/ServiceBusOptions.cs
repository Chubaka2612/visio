namespace Visio.Services.Notifications
{
    public class ServiceBusOptions
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }

        public ServiceBusOptions() { }

        public ServiceBusOptions(string connectionString, string queueName)
        {
            ArgumentNullException.ThrowIfNull(connectionString);
            ArgumentNullException.ThrowIfNull(queueName);

            ConnectionString = connectionString;
            QueueName = queueName;
        }
    }
}