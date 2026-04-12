using Academy.Shared.DTO;
using Academy.Shared.DTO.DBSchema;
using Academy.Shared.Enums;

namespace Academy.Core.Abstraction.Services
{
    public interface IAIService
    {
        Task<AIQuery> GetAISQLQuery(string aiModel, AIServices aiService, string userPrompt, DatabaseSchema dbSchema, string databaseType);
    }
}
