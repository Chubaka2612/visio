namespace Visio.Services.Notifications
{
    public interface INotificationProducer
    {
        Task PublishMessageAsync<T>(T message, string queueName);
    }
}
