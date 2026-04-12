using Academy.Shared.DTO;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Staffing.Core.Abstraction.Infrastructure
{
    public interface IChatHistoryRepository
    {
        Task SaveAsync(string sessionId, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
        Task DeleteBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
        Task<ChatHistory?> LoadAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
