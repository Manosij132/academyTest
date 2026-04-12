using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.PredicateBuilder;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Exceptions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Academy.Core.Services
{
    //Only System Admin can make additions or edits to Seniority
    public class SeniorityService : ISeniorityService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IAcademyDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SeniorityMaster> _repositorySeniority;
        private readonly IPredicateFactory _predicateFactory;
        private readonly AbstractAdminPredicate predicateBuilder;
        public SeniorityService(IAuthenticatedUserService authenticatedUserService, IAcademyDbContext dbContext, IUnitOfWork unitOfWork, IPredicateFactory predicateFactory)
        {
            _authenticatedUserService = authenticatedUserService;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _repositorySeniority = _unitOfWork.GetRepository<SeniorityMaster>();
            _predicateFactory = predicateFactory;
            predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
        }
        public async Task<Result<List<SeniorityDto>>> Fetch()
        {
            var result = await _dbContext.SeniorityMasters.Select(x => x).ToListAsync();

            List<SeniorityDto> response = result.Select(s => new SeniorityDto()
            {
                Id = s.SeniorityId,
                IsActive = s.IsActive,
                Level = s.SeniorityLevel,
                Name = s.SeniorityName
            }).ToList();
            return response;
        }
        public async Task<Result<string>> Insert(SeniorityDto request)
        {
            bool isPermitted = predicateBuilder.CanInsertOrUpdateSeniority();
            if (!isPermitted)
            {
                // Return an error message if the user is not authorized
                return Result.Failure<string>(DomainErrors.Authorization.UnauthorizedAccess);
            }
            SeniorityMaster seniority = new()
            {
                CreatedBy = _authenticatedUserService.AuthUser.Id,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                SeniorityLevel = request.Level,
                SeniorityName = request.Name
            };

            // Insert the new seniority record into the repository
            var result = await _repositorySeniority.InsertAsync(seniority);

            // Save changes to the unit of work and get the response
            int response = await _unitOfWork.SaveChangesAsync();

            // Return a success message if more than one record was affected, otherwise return an error message
            return response > 1 ? Messages.SUCCESS_GENERIC : Messages.ERROR_Generic;
        }

        public async Task<Result<string>> Modify(SeniorityDto request)
        {
            bool isPermitted = predicateBuilder.CanInsertOrUpdateSeniority();
            if (!isPermitted)
            {
                // Return an error message if the user is not authorized
                return Result.Failure<string>(DomainErrors.Authorization.UnauthorizedAccess);
            }
            SeniorityMaster seniority = await _repositorySeniority.GetFirstOrDefaultAsync(predicate: s => s.SeniorityId == request.Id);
            if (seniority is null)
            {
                return Result.Failure<string>(DomainErrors.Common.NotFound(request.Id.ToString()));
            }

            seniority.SeniorityLevel = request.Level;
            seniority.SeniorityName = request.Name;
            seniority.UpdatedOn = DateTime.UtcNow;
            seniority.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            _repositorySeniority.Update(seniority);

            // Save changes to the unit of work and get the response
            int response = await _unitOfWork.SaveChangesAsync();

            // Return a success message if more than one record was affected, otherwise return an error message
            return response > 1 ? Messages.SUCCESS_GENERIC : Messages.ERROR_Generic;
        }

        public async Task<Result<string>> Deactivate(short seniorityId)
        {
            bool isPermitted = predicateBuilder.CanInsertOrUpdateSeniority();
            if (!isPermitted)
            {
                // Return an error message if the user is not authorized
                return Result.Failure<string>(DomainErrors.Authorization.UnauthorizedAccess);
            }
            SeniorityMaster seniority = await _repositorySeniority.GetFirstOrDefaultAsync(predicate: s => s.SeniorityId == seniorityId);
            if (seniority is null)
            {
                return Result.Failure<string>(DomainErrors.Common.NotFound(seniorityId.ToString()));
            }

            seniority.IsActive = false;
            seniority.UpdatedOn = DateTime.UtcNow;
            seniority.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            _repositorySeniority.Update(seniority);

            // Save changes to the unit of work and get the response
            int response = await _unitOfWork.SaveChangesAsync();

            // Return a success message if more than one record was affected, otherwise return an error message
            return response > 1 ? Messages.SUCCESS_GENERIC : Messages.ERROR_Generic;
        }
    }
}
