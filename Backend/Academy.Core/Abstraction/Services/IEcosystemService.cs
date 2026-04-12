using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Response;

namespace Academy.Core.Abstraction.Services
{
    public interface IEcosystemService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all active EcosystemDto entities from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a list of EcosystemDto objects.</returns>
        Task<Result<List<EcosystemDto>>> FetchAllEcosystem(bool includePrimary = true);
        /// <summary>
        /// Asynchronously inserts a new Secondary Ecosystem into the database based on the provided EcosystemDto request.
        /// </summary>
        /// <param name="request">The EcosystemDto object containing the details of the ecosystem to be inserted.</param>
        /// <returns>A task that represents the asynchronous operation, containing a message indicating the result of the insert operation.</returns>
        Task<Result<string>> InsertEcosystem(EcosystemDto request);

        Task<Result<List<string>>> FetchAllPrimaryEcosystems();
        Task<int?> FetchEcoSystemIdByName(string name);
    }
}
