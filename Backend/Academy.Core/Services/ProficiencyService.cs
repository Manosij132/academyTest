using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Core.PredicateBuilder;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using static Academy.Shared.Exceptions.DomainErrors;

namespace Academy.Core.Services
{
    public class ProficiencyService : IProficiencyService
    {
        private readonly IPredicateFactory _predicateFactory;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IEmployeeService _employeeService;
        private readonly IAcademyDbContext _dbContext;
        private readonly IRepository<SkillEndorsementMap> _repositorySkillEndorsementMap;
        private readonly IRepository<ProficiencyMaster> _repositoryProficiency;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdoClient<AcademyDbSetting> _academyDbAdoClient;
        private readonly AbstractAdminPredicate predicateBuilder;

        public ProficiencyService(IAuthenticatedUserService authenticatedUserService, IUnitOfWork unitOfWork, IEmployeeService employeeService,
            IAdoClient<AcademyDbSetting> academyDbAdoClient, IPredicateFactory predicateFactory, IAcademyDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _authenticatedUserService = authenticatedUserService;
            _employeeService = employeeService;
            _academyDbAdoClient = academyDbAdoClient;
            _repositorySkillEndorsementMap = _unitOfWork.GetRepository<SkillEndorsementMap>();
            _repositoryProficiency = _unitOfWork.GetRepository<ProficiencyMaster>();
            _predicateFactory = predicateFactory;
            if (_authenticatedUserService.AuthUser != null)
            {
                predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            }
            _dbContext = dbContext;
        }

        public async Task<Result<int>> InsertOrUpdateEmployeeProficiency(ProficiencyRequest request)
        {
            if (request == null)
                return Result.Failure<int>(Common.NullOrEmptyValue(nameof(request)));

            // Get the Predicate based on Logged in User Role
            request.LoggedInUserId = _authenticatedUserService.AuthUser.Id;
            var result = await _employeeService.FetchById(request.EmployeeId);

            if (result.IsFailure)
                return Result.Failure<int>(result.Error);

            bool isPermitted = predicateBuilder.CanInsertOrUpdateProficiency(result.Value);

            if (!isPermitted)
                return Result.Failure<int>(Authorization.UnauthorizedAccess);

            Expression<Func<SkillEndorsementMap, bool>> predicate = x => x.EmployeeId.Equals(request.EmployeeId) && x.SkillId.Equals(request.SkillId) && x.IsActive;

            var exsistingEntry = await _repositorySkillEndorsementMap.GetFirstOrDefaultAsync(predicate: predicate);

            if (exsistingEntry != null)
            {
                if (request.NewKnowledge == 0)
                    request.NewKnowledge = exsistingEntry.CurrentKnowledge;
                if (request.NewProficiency == 0)
                    request.NewProficiency = exsistingEntry.CurrentProficiency;


                exsistingEntry.IsActive = false;
                exsistingEntry.UpdatedOn = DateTime.UtcNow;
                exsistingEntry.UpdatedBy = request.LoggedInUserId;

                _repositorySkillEndorsementMap.Update(exsistingEntry);
            }

            SkillEndorsementMap newEntry = new()
            {
                CurrentProficiency = request.NewProficiency,
                CurrentKnowledge = request.NewKnowledge,
                IsActive = true,
                SkillId = request.SkillId,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = request.LoggedInUserId,
                EmployeeId = request.EmployeeId
            };

            await _repositorySkillEndorsementMap.InsertAsync(newEntry);

            int count = await _unitOfWork.SaveChangesAsync();
            return count;
        }

        public async Task<Result<List<SkillEndorsementResponse>>> FetchProficienciencies(int employeeId)
        {
            List<SkillEndorsementResponse> list = [];

            var result = await _employeeService.FetchById(employeeId);

            if (result.IsFailure)
                return Result.Failure<List<SkillEndorsementResponse>>(result.Error);

            var employee = result.Value;
            Dictionary<string, object> iParams = new()
            {
                { DbConstants.PARAM_EMPLOYEE_ID, employee.Id }
            };

            DataTable table = await _academyDbAdoClient.ExecuteReaderAsync(DbConstants.FETCH_PROFICIENCIES, iParams);
            list = table.ToList<SkillEndorsementResponse>();
            return list;
        }

        public async Task<Result<List<ProficiencyDto>>> Fetch()
        {
            Expression<Func<ProficiencyMaster, bool>> predicate = p => p.IsActive;
            int count = _repositoryProficiency.Count(predicate);
            var items = await _repositoryProficiency.GetPagedListAsync(predicate: predicate, pageSize: count, pageIndex: 0);
            List<ProficiencyDto> result = items.Items.Select(x => new ProficiencyDto()
            {
                IsActive = x.IsActive,
                ProficiencyId = x.ProficiencyId,
                ProficiencyLevel = x.ProficiencyRating,
                KnowledgeLevel = x.ProficiencyRating,
                ProficiencyName = x.ProficiencyName
            }).ToList();

            return result;
        }

        public async Task<Result<List<ProficiencyDto>>> FetchProficiencyByEcosystemSkill(short ecosystemId, short skillId)
        {
            //TODO: AK: Debug why QC eco and ETL skills breaks code.
            List<ProficiencyDto> response = [];
            var result = await (from t in _dbContext.TrainingProficiencyMaps
                                join p in _dbContext.ProficiencyMasters
                                on t.ExpectedProficiency equals p.ProficiencyRating
                                join s in _dbContext.SeniorityMasters
                                on t.SeniorityId equals s.SeniorityId
                                where t.IsActive && p.IsActive && t.EcosystemId == ecosystemId && t.SkillId == skillId
                                select new ProficiencyDto()
                                {
                                    IsActive = true,
                                    ProficiencyId = p.ProficiencyId,
                                    ProficiencyLevel = t.ExpectedProficiency,
                                    KnowledgeLevel = t.ExpectedKnowledge,
                                    ProficiencyName = p.ProficiencyName,
                                    SeniorityId = t.SeniorityId,
                                    SeniorityName = s.SeniorityName,
                                    IsMVP = t.IsMVP
                                }).Distinct().ToListAsync();

            foreach (var item in result)
            {
                if (response.Any(x => x.SeniorityId == item.SeniorityId))
                {
                    response.First(x => x.SeniorityId == item.SeniorityId).SeniorityName += $" | {item.SeniorityName}";
                }

                else
                {
                    response.Add(new()
                    {
                        IsActive = item.IsActive,
                        ProficiencyId = item.ProficiencyId,
                        ProficiencyLevel = item.ProficiencyLevel,
                        KnowledgeLevel = item.ProficiencyLevel,
                        ProficiencyName = item.ProficiencyName,
                        SeniorityId = item.SeniorityId,
                        SeniorityName = item.SeniorityName
                    });
                }
            }
            return Result.Success(response);
        }
    }
}