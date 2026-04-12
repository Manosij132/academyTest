using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Academy.Infrastructure.AzureStorageClient
{
    public class Queue : IStorageQueue
    {
        private readonly Uri _connectionString;
        public Queue(IAzureStorageSettings azureStorageSettings)
        {
            _connectionString = new(azureStorageSettings?.ConnectionString);
        }
        public async Task SendMessageAsync(string queueName, string message)
        {
            QueueClient queueClient = new(_connectionString.ToString(), queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);
        }

        public async Task<string> ReceiveMessageAsync(string queueName)
        {
            QueueClient queueClient = new(_connectionString.ToString(), queueName);
            QueueProperties properties = queueClient.GetProperties();
            if (properties.ApproximateMessagesCount > 0)
            {
                QueueMessage[] retrievedMessage = await queueClient.ReceiveMessagesAsync(1);
                string theMessage = retrievedMessage[0].Body.ToString();
                await queueClient.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
                return theMessage;
            }
            else return default;
        }
    }
}
