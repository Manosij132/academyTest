namespace Academy.Core.Abstraction.Infrastructure
{
    public interface IStorageQueue
    {
        Task SendMessageAsync(string queueName, string message);
        Task<string> ReceiveMessageAsync(string queueName);
    }
}
