

using Academy.Core.Abstraction.Infrastructure;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.ChatCompletion;
using Staffing.Core.Abstraction.Infrastructure;

namespace Staffing.Core.Repositories
{
    internal class ChatHistoryRepository : IChatHistoryRepository
    {
        private readonly IAcademyDbContext _db;

        public ChatHistoryRepository(IAcademyDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(string sessionId, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            // Remove previous messages for this session to avoid duplicates, then insert current snapshot
            var existing = _db.UserChatMessageHistorys.Where(x => x.SessionId == sessionId);
            _db.UserChatMessageHistorys.RemoveRange(existing);

            var toInsert = messages.Select(m => new UserChatMessageHistory
            {
                SessionId = sessionId,
                Role = m.Role,
                Content = m.Content,
                Timestamp = DateTime.UtcNow
            }).ToList();

            if (toInsert.Any())
                await _db.UserChatMessageHistorys.AddRangeAsync(toInsert, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            var existing = _db.UserChatMessageHistorys.Where(x => x.SessionId == sessionId);
            _db.UserChatMessageHistorys.RemoveRange(existing);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<ChatHistory?> LoadAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            var rows = await _db.UserChatMessageHistorys
                                .Where(x => x.SessionId == sessionId)
                                .OrderBy(x => x.Timestamp)
                                .ToListAsync(cancellationToken);

            if (!rows.Any()) return null;

            var history = new ChatHistory();
            foreach (var r in rows)
            {
                switch (r.Role?.ToLowerInvariant())
                {
                    case "system":
                        history.AddSystemMessage(r.Content);
                        break;
                    case "assistant":
                        history.AddSystemMessage(r.Content);
                        break;
                    case "user":
                    default:
                        history.AddUserMessage(r.Content);
                        break;
                }
            }
            return history;
        }
    }
}