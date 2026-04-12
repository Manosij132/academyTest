using Academy.Shared.DTO.DBSchema;

namespace Academy.Core.Abstraction.Infrastructure
{
    public interface ISchemaInspector
    {
        Task<DatabaseSchema> GenerateSchemaAsync();
    }
}
