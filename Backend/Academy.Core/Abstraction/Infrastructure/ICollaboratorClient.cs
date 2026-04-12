namespace Academy.Core.Abstraction.Infrastructure
{
    public interface ICollaboratorClient
    {
        Task SendMessageAsync(dynamic message);
    }
}
