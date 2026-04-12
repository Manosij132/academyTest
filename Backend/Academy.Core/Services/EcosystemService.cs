using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.PredicateBuilder;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Exceptions;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using System.Linq.Expressions;

namespace Academy.Core.Services
{
    public class EcosystemService : IEcosystemService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IAcademyDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EcosystemMaster> _repositoryEcosystem;
        private readonly IPredicateFactory _predicateFactory;
        private readonly AbstractAdminPredicate predicateBuilder;
        public EcosystemService(IAuthenticatedUserService authenticatedUserService, IAcademyDbContext dbContext, IUnitOfWork unitOfWork, IPredicateFactory predicateFactory)
        {
            _authenticatedUserService = authenticatedUserService;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _repositoryEcosystem = _unitOfWork.GetRepository<EcosystemMaster>();
            _predicateFactory = predicateFactory;
            if (_authenticatedUserService.AuthUser != null)
            {
                predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of all active EcosystemMaster entities from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a list of EcosystemMaster objects.</returns>
        public async Task<Result<List<EcosystemDto>>> FetchAllEcosystem(bool includePrimary = true)
        {
            List<EcosystemDto> list = [];
            Expression<Func<EcosystemMaster, bool>> predicate = predicateBuilder.FetchAndInsertEcosystems(_dbContext);

            if (predicate is not null)
            {
                // Count the total number of EcosystemMaster entities that match the given predicate.
                int count = _repositoryEcosystem.Count(predicate);

                // Asynchronously retrieve a paginated list of EcosystemMaster entities that match the predicate.
                // The page size is set to the total count to fetch all matching records in one page,
                // starting from the first page (index 0).
                var result = await _repositoryEcosystem.GetPagedListAsync(predicate: predicate, pageSize: count, pageIndex: 0);

                // Extract the list of EcosystemMaster entities from the result.
                var ecosystemList = result.Items;

                // Convert the list of EcosystemMaster entities to a standard List and return it.
                if (includePrimary)
                {
                    list = ecosystemList.Select(x => new EcosystemDto()
                    {
                        Name = x.EcosystemName,
                        Id = x.EcosystemId,
                        PrimaryEcosystemId = x.ParentEcosystemId,
                        IsPrimary = x.IsPrimary,
                        IsActive = x.IsActive
                    }).ToList();
                }
                else
                {
                    list = ecosystemList.Where(x => !x.IsPrimary).Select(x => new EcosystemDto()
                    {
                        Name = x.EcosystemName,
                        Id = x.EcosystemId,
                        PrimaryEcosystemId = x.ParentEcosystemId,
                        IsPrimary = x.IsPrimary,
                        IsActive = x.IsActive
                    }).ToList();
                }
            }

            return list;
        }
        /// <summary>
        /// Asynchronously inserts a new Secondary Ecosystem into the database based on the provided EcosystemDto request.
        /// </summary>
        /// <param name="request">The EcosystemDto object containing the details of the ecosystem to be inserted.</param>
        /// <returns>A task that represents the asynchronous operation, containing a message indicating the result of the insert operation.</returns>
        public async Task<Result<string>> InsertEcosystem(EcosystemDto request)
        {
            Expression<Func<EcosystemMaster, bool>> predicate = predicateBuilder.FetchAndInsertEcosystems(_dbContext);
            if (predicate is not null)
            {
                // Create a new instance of EcosystemMaster and initialize its properties.
                EcosystemMaster ecosystemMaster = new()
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true,
                    IsPrimary = false,
                    EcosystemName = request.Name,
                    ParentEcosystemId = request.PrimaryEcosystemId,
                };

                // Asynchronously insert the new EcosystemMaster instance into the repository and store the result.
                var result = await _repositoryEcosystem.InsertAsync(ecosystemMaster);

                // Save changes to the unit of work asynchronously and get the response count.
                var response = await _unitOfWork.SaveChangesAsync();

                // Return a success message if the operation resulted in more than one change saved; 
                // otherwise, return a detailed error message indicating an issue occurred during the save operation.
                return response > 1 ? Messages.SUCCESS_GENERIC : Messages.ERROR_Generic;
            }
            else
                return Result.Failure<string>(DomainErrors.Authorization.UnauthorizedAccess);
        }

        public async Task<Result<List<string>>> FetchAllPrimaryEcosystems()
        {
            List<string> data = [];
            Expression<Func<EcosystemMaster, bool>> predicate = x => x.IsActive && x.IsPrimary;
            int count = _repositoryEcosystem.Count(predicate);
            var pagedData = await _repositoryEcosystem.GetPagedListAsync(
                predicate: x => x.IsActive && x.IsPrimary,
                pageIndex: 0, pageSize: count,
                orderBy: list => list.OrderBy(y => y.EcosystemName));
            data = pagedData.Items.Select(x => x.EcosystemName).ToList();
            return data;
        }
        public async Task<int?> FetchEcoSystemIdByName(string name)
        {
            var predicate = (Expression<Func<EcosystemMaster, bool>>)
                (x => x.IsActive && x.IsPrimary && x.EcosystemName.ToLower().Trim() == name.ToLower().Trim());

            var result = await _repositoryEcosystem.GetPagedListAsync(
                predicate: predicate,
                pageIndex: 0,
                pageSize: 1,
                orderBy: q => q.OrderBy(x => x.EcosystemName)
            );

            if (result.TotalCount == 0)
            {
                return null;
            }

            return result.Items.FirstOrDefault().EcosystemId;
        }
    }
}