using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Core.PredicateBuilder;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Exceptions;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Linq.Expressions;

namespace Academy.Core.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IEmployeeService _employeeService;
        private readonly IEcosystemService _ecosystemService;
        private readonly IUnitOfWork _unitOfWork;
        public readonly IRepository<SkillMaster> _repositorySkillMaster;
        public readonly IRepository<TrainingMaster> _repositoryTrainingMaster;
        public readonly IRepository<TrainingProficiencyMap> _repositoryTrainingProficiencyMap;
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IAdoClient<AcademyDbSetting> _adoClient;
        private readonly IRepository<CategoryMaster> _repositoryCategoryMaster;
        private readonly IPredicateFactory _predicateFactory;
        private readonly AbstractAdminPredicate predicateBuilder;
        public TrainingService(IAuthenticatedUserService authenticatedUserService, IUnitOfWork unitOfWork, IEmployeeService employeeService,
             IAcademyDbContext academyDbContext, IAdoClient<AcademyDbSetting> adoClient, IEcosystemService ecosystemService, IPredicateFactory predicateFactory)
        {
            _unitOfWork = unitOfWork;
            _authenticatedUserService = authenticatedUserService;
            _employeeService = employeeService;
            _repositorySkillMaster = _unitOfWork.GetRepository<SkillMaster>();
            _repositoryTrainingMaster = _unitOfWork.GetRepository<TrainingMaster>();
            _repositoryTrainingProficiencyMap = _unitOfWork.GetRepository<TrainingProficiencyMap>();
            _academyDbContext = academyDbContext;
            _adoClient = adoClient;
            _ecosystemService = ecosystemService;
            _repositoryCategoryMaster = _unitOfWork.GetRepository<CategoryMaster>();
            _predicateFactory = predicateFactory;
            predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
        }

        public async Task<Result<FetchTrainingListResponse>> FetchTrainingList(FetchTrainingListRequest request)
        {
            FetchTrainingListResponse response = new();

            Expression<Func<TrainingMaster, bool>> predicate = x => x.IsActive;

            if (!string.IsNullOrEmpty(request.SearchTearm))
            {
                predicate = predicate.And(x => x.TrainingName.Contains(request.SearchTearm, StringComparison.CurrentCultureIgnoreCase));
            }

            int count = _repositoryTrainingMaster.Count(predicate: predicate);
            IPagedList<TrainingMaster> Training_result = await _repositoryTrainingMaster
                .GetPagedListAsync(
                predicate: predicate,
                pageIndex: request.PageIndex,
                pageSize: request.PageSize,
                orderBy: list => list.OrderBy(y => y.TrainingName));
            List<Training> trainings = [.. Training_result.Items.Select(x =>
            new Training()
            {
                TrainingId = x.TrainingId,
                TrainingName = x.TrainingName,
                IsActive = x.IsActive,
                IsPriortize = x.IsPriortize,
                TrainingCompletionHours = x.TrainingCompletionHours,
                TrainingDescription = x.TrainingDescription,
                TrainingUrl = x.TrainingUrl,
            })];

            response.TotalRecords = count;
            response.TrainingList = trainings;

            return Result.Success(response);
        }

        public async Task<Result<int>> UpdateTraining(UpdateTrainingRequest request)
        {

            bool isPermitted = predicateBuilder.CanCreateOrUpdateTrainings();
            if (!isPermitted)
            {
                return Result.Failure<int>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            var exsistingTraining = await _repositoryTrainingMaster.GetFirstOrDefaultAsync(predicate: x => x.TrainingId.Equals(request.TrainingId));
            if (exsistingTraining == null)
            {
                return Result.Failure<int>(DomainErrors.Common.NotFound(request.TrainingId.ToString()));
            }
            exsistingTraining.UpdatedOn = DateTime.UtcNow;
            exsistingTraining.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            exsistingTraining.IsPriortize = request.IsPriortize;

            _repositoryTrainingMaster.Update(exsistingTraining);

            var result = await _unitOfWork.SaveChangesAsync();
            return Result.Success(result);
        }
    }
}
