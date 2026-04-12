using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Core.PredicateBuilder;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Exceptions;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace Academy.Core.Services
{
    public class SkillAndTrainingService : ISkillAndTrainingService
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
        // public readonly IRepository<ReportColumnConfiguration> _repositoryReportColumnConfiguration;
        public SkillAndTrainingService(IAuthenticatedUserService authenticatedUserService, IUnitOfWork unitOfWork, IEmployeeService employeeService,
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

        public async Task<Result<int>> InsertOrUpdateSkill(SkillDto request)
        {
            bool isPermitted = predicateBuilder.CanInsertOrUpdateSkill();
            if (!isPermitted)
            {
                return Result.Failure<int>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            if (request == null)
                return Result.Failure<int>(DomainErrors.Common.NullOrEmptyValue("SkillDto"));

            if (request.SkillId == null || request.SkillId == 0)
            {
                request.SkillId = short.Parse(_academyDbContext.SkillMasters.Select(x => x.SkillId).Max().ToString());
                request.SkillId += 1;
                SkillMaster skill = new()
                {
                    SkillId = request.SkillId.Value,
                    SkillName = request.SkillName,
                    DisplayName = request.SkillName,
                    SkillDescription = request.SkillDescription ?? string.Empty,
                    IsActive = request.IsActive,
                    Mandatory = request.Mandatory,
                    Grouping = request.Grouping,
                    Specification = request.Specification,
                    CategoryId = request.CategoryId,
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow
                };

                var response = await _repositorySkillMaster.InsertAsync(skill);
            }
            else
            {
                var ExsistingSkill = await _repositorySkillMaster.GetFirstOrDefaultAsync(predicate: x => x.SkillId.Equals(request.SkillId));
                if (ExsistingSkill != null)
                {
                    return Result.Failure<int>(DomainErrors.Common.NotFound(request.SkillId.ToString()));
                }
                ExsistingSkill.UpdatedOn = DateTime.UtcNow;
                ExsistingSkill.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                ExsistingSkill.SkillName = request.SkillName;
                ExsistingSkill.Mandatory = request.Mandatory;
                ExsistingSkill.Grouping = request.Grouping;
                ExsistingSkill.CategoryId = request.CategoryId;
                ExsistingSkill.SkillDescription = request.SkillDescription;
                ExsistingSkill.Specification = request.Specification;
                ExsistingSkill.IsActive = request.IsActive;

                _repositorySkillMaster.Update(ExsistingSkill);
            }

            var result = await _unitOfWork.SaveChangesAsync();

            return Result.Success(result);
        }

        public async Task<Result<int>> InsertOrUpdateTraining(TrainingProficiencyDto request)
        {
            bool isPermitted = predicateBuilder.CanInsertOrUpdateTraining();
            if (!isPermitted)
            {
                return Result.Failure<int>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            if (request == null)
                return Result.Failure<int>(DomainErrors.Common.NullOrEmptyValue("TrainingProficiencyDto"));

            if (request == null)
            {
                TrainingMaster training = new()
                {
                    TrainingName = request.TrainingName,
                    TrainingUrl = request.TrainingLink,
                    TrainingCompletionHours = request.TrainingCompletionHours,
                    IsActive = request.IsActive,
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow
                };

                var response = await _repositoryTrainingMaster.InsertAsync(training);
            }
            else
            {
                var existingTraining = await _repositoryTrainingMaster.GetFirstOrDefaultAsync(predicate: x => x.TrainingId.Equals(request.TrainingId));
                if (existingTraining != null)
                {
                    return Result.Failure<int>(DomainErrors.Common.NotFound(request.TrainingId.ToString()));
                }

                existingTraining.UpdatedOn = DateTime.UtcNow;
                existingTraining.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                existingTraining.TrainingName = request.TrainingName;
                existingTraining.TrainingUrl = request.TrainingLink;
                existingTraining.IsActive = request.IsActive;
                existingTraining.TrainingCompletionHours = request.TrainingCompletionHours;
                _repositoryTrainingMaster.Update(existingTraining);
            }
            var result = await _unitOfWork.SaveChangesAsync();

            return Result.Success(result);
        }

        public async Task<Result<int>> InsertTrainingProficiencyMapping(TrainingProficiencyDto request)
        {
            bool isPermitted = predicateBuilder.CanInsertTrainingProficiencyMapping();
            if (!isPermitted)
            {
                return Result.Failure<int>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            if (request == null)
                return Result.Failure<int>(DomainErrors.Common.NullOrEmptyValue("TrainingProficiencyDto"));

            Expression<Func<TrainingProficiencyMap, bool>> predicate = x => x.EcosystemId == request.EcosystemId
                                                                            && x.SkillId == request.SkillId
                                                                            && x.SeniorityId == request.SeniorityId;
            TrainingProficiencyMap data = await _repositoryTrainingProficiencyMap.GetFirstOrDefaultAsync(predicate: predicate);

            if (data == null)
            {
                data = new()
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    EcosystemId = request.EcosystemId,
                    ExpectedProficiency = request.ExpectedProficiency,
                    IsActive = request.IsActive,
                    IsMVP = request.IsMvP,
                    SeniorityId = request.SeniorityId,
                    SkillId = request.SkillId,
                    TrainingId = request.TrainingId,
                };
            }
            else
            {
                if (data.TrainingId.Equals(request.TrainingId))
                {
                    return await UpdateTrainingProficiency(request);
                }
                data = new()
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    EcosystemId = request.EcosystemId,
                    ExpectedProficiency = data.ExpectedProficiency,
                    IsActive = request.IsActive,
                    IsMVP = request.IsMvP,
                    SeniorityId = request.SeniorityId,
                    SkillId = request.SkillId,
                    TrainingId = request.TrainingId,
                };
            }
            await _repositoryTrainingProficiencyMap.InsertAsync(data);
            var result = await _unitOfWork.SaveChangesAsync();

            return Result.Success(result);
        }

        public async Task<Result<int>> UpdateTrainingProficiency(TrainingProficiencyDto request)
        {
            bool isPermitted = predicateBuilder.CanUpdateTrainingProficiency();
            if (!isPermitted)
            {
                return Result.Failure<int>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            if (request == null || request.SkillId == null)
                return Result.Failure<int>(DomainErrors.Common.NullOrEmptyValue("TrainingProficiencyDto"));

            List<TrainingProficiencyMap> data = (from x in _academyDbContext.TrainingProficiencyMaps
                                                 where x.SkillId == request.SkillId
                                                 && x.EcosystemId == request.EcosystemId
                                                 && x.SeniorityId == request.SeniorityId
                                                 select x).ToList();
            if (data == null)
                return Result.Failure<int>(DomainErrors.Common.NullOrEmptyValue("List<TrainingProficiencyMap>"));

            data.ForEach(x =>
            {
                x.ExpectedProficiency = request.ExpectedProficiency;
                x.UpdatedOn = DateTime.UtcNow;
                x.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                _repositoryTrainingProficiencyMap.Update(x);
            });
            var result = await _unitOfWork.SaveChangesAsync();

            return Result.Success(result);
        }

        public async Task<Result<List<SkillDto>>> FetchSkills()
        {
            List<SkillDto> response = [];
            Expression<Func<SkillMaster, bool>> predicate = x => x.IsActive;
            int count = _repositorySkillMaster.Count(predicate);
            var items = await _repositorySkillMaster.GetPagedListAsync(predicate: predicate, pageSize: count, pageIndex: 0, orderBy: x => x.OrderBy(y => y.SkillName));
            response = items.Items.Select(x => new SkillDto()
            {
                SkillId = x.SkillId,
                SkillName = x.SkillName,
                CategoryId = x.CategoryId,
                Grouping = x.Grouping,
                Mandatory = x.Mandatory,
                SkillDescription = x.SkillDescription,
                Specification = x.Specification,
                IsActive = x.IsActive,
            }).ToList();
            return Result.Success(response);
        }

        public async Task<Result<List<TrainingsGroupedBySkill>>> FetchSkillTrainingsMetaData(int ecosystemId)
        {
            List<TrainingsGroupedBySkill> response = new();
            Dictionary<string, object> iParam = new()
            {
                { DbConstants.PARAM_ECOSYSTEM_ID, ecosystemId }
            };
            var table = await _adoClient.ExecuteReaderAsync(DbConstants.FETCH_SKILL_TRAININGS_METADATA, iParam);
            if (table != null)
            {
                List<SkillTrainingDto> result = table.ToList<SkillTrainingDto>();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        if (!response.Any(x => x.SkillId == item.SkillId))
                        {
                            TrainingsGroupedBySkill parent = new()
                            {
                                SkillId = item.SkillId,
                                SkillName = item.SkillName,
                                EcosystemId = item.EcosystemId,
                                ExpectedProficiency = item.ExpectedProficiency,
                                ExpectedKnowledge = item.ExpectedKnowledge
                            };
                            response.Add(parent);
                        }
                        response.FirstOrDefault(x => x.SkillId.Equals(item.SkillId)).Trainings.Add(
                            new TrainingMasterResponse()
                            {
                                IsMvP = item.IsMvP,
                                TrainingId = item.TrainingId,
                                SeniorityId = item.SeniorityId,
                                Seniority = ApplicationConstants.ALLOWED_SENIORITIES_DETAILS.FirstOrDefault(x => x.Key.Value == item.SeniorityId).Value,
                                TrainingName = item.TrainingName,
                            }
                        );
                    }
                }
            }
            return Result.Success(response);
        }
        public async Task<Result<List<BaseSkillEndorsementResponse>>> FetchSkillEndorsement(short ecosystemId, string account, string commaSeperatedUserIds)
        {
            List<BaseSkillEndorsementResponse> response = new();
            if (string.IsNullOrWhiteSpace(commaSeperatedUserIds))
            {
                var result = await _employeeService.FetchByEcosystemAndEmailStartsWith(string.Empty, ecosystemId, account);

                if (result.IsFailure)
                {
                    return Result.Failure<List<BaseSkillEndorsementResponse>>(result.Error);
                }

                var employees = result.Value;
                var employeeIds = employees.Select(x => x.EmployeeId).ToList();
                commaSeperatedUserIds = string.Join(",", employeeIds);
            }

            // Need to create an XML that contains multiple userIds, like: 
            // <root><user>12170</userId><user>387</userId></root>
            List<string> userIds = commaSeperatedUserIds.Split(',').ToList();
            XElement root = new(nameof(root));
            foreach (string userId in userIds)
            {
                XElement user = new(nameof(user), userId);
                root.Add(user);
            }
            Dictionary<string, object> iParam = new()
            {
                { DbConstants.PARAM_EMPLOYEE_ID, root.ToString() }
            };
            var table = await _adoClient.ExecuteReaderAsync(DbConstants.FETCH_SKILL_ENDORSEMENT, iParam);
            if (table != null)
            {
                response = table.ToList<BaseSkillEndorsementResponse>();
                if (response != null)
                {
                    return response;
                }
            }
            return Result.Success(response);
        }
        public async Task<Result<string>> CreateTrainings(ManageTrainingDto request)
        {
            bool isPermitted = predicateBuilder.CanCreateOrUpdateTrainings();
            if (!isPermitted)
            {
                return Result.Failure<string>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            var result = await _ecosystemService.FetchAllEcosystem();

            if (result.IsFailure)
                return Result.Failure<string>(result.Error);


            Expression<Func<TrainingMaster, bool>> trainingPredicate = x => x.TrainingId.Equals(request.trainingId)
                                                                       || x.TrainingUrl.Equals(request.trainingUrl)
                                                                        && x.IsActive;

            var existingTraining = await _repositoryTrainingMaster.GetFirstOrDefaultAsync(predicate: trainingPredicate);

            //if (request.trainingId <= 0)
            //{
            //    request.trainingId = short.Parse(_academyDbContext.TrainingMasters.Select(x => x.TrainingId).Max().ToString()) + 1;
            //}

            var ecosystems = result.Value;
            var ecosystem = ecosystems.FirstOrDefault(x => x.Id == request.ecosystemId);
            var skill = await _repositorySkillMaster.GetFirstOrDefaultAsync(predicate: x => x.SkillId == request.skillId);

            List<TrainingProficiencyMap> data = [];

            if (ecosystem is null) return Result.Failure<string>(DomainErrors.Common.NullOrEmptyValue("Ecosystem"));
            if (skill is null) return Result.Failure<string>(DomainErrors.Common.NullOrEmptyValue("Skill"));

            foreach (var item in request.expectedProficiency)
            {
                TrainingProficiencyMap map = new()
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true,
                    EcosystemId = request.ecosystemId,
                    SkillId = (short)request.skillId,
                    TrainingId = request.trainingId,
                    IsMVP = request.IsMvP
                };
                Expression<Func<TrainingProficiencyMap, bool>> predicate = x => x.SkillId.Equals(request.skillId)
                                                                        && x.EcosystemId.Equals(Convert.ToInt32(request.ecosystemId))
                                                                        && x.SeniorityId.Equals(item.seniorityId) && x.IsActive;

                var existing = await _repositoryTrainingProficiencyMap.GetFirstOrDefaultAsync(predicate: predicate);

                if (existing is null)
                {
                    map.ExpectedProficiency = item.proficiencyValue;
                    map.SeniorityId = item.seniorityId;
                    map.ExpectedKnowledge = item.knowledgeValue;
                }
                else
                {
                    map.SeniorityId = existing.SeniorityId;
                    map.ExpectedProficiency = existing.ExpectedProficiency;
                    map.ExpectedKnowledge = existing.ExpectedKnowledge;
                }

                data.Add(map);
            }

            if (existingTraining is null)
            {
                var training = new TrainingMaster
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    TrainingCompletionHours = (short)request.trainingCompletionHours,
                    TrainingId = request.trainingId,
                    TrainingName = request.trainingName,
                    TrainingDescription = request.trainingDescription,
                    TrainingUrl = request.trainingUrl,
                    IsActive = true,
                    IsPriortize = request.IsPriortize
                };

                await _repositoryTrainingMaster.InsertAsync([training]);
            }

            await _repositoryTrainingProficiencyMap.InsertAsync(data);
            int count = await _unitOfWork.SaveChangesAsync();
            return Result.Success($"Total rows affected: {count}");
        }
        public async Task<Result<List<CategoryDto>>> FetchCategory()
        {
            List<CategoryDto> response = [];
            Expression<Func<CategoryMaster, bool>> predicate = x => x.IsActive && x.ParentCategoryId == null;
            int count = _repositoryCategoryMaster.Count(predicate: predicate);
            IPagedList<CategoryMaster> categories_result = await _repositoryCategoryMaster
                .GetPagedListAsync(
                predicate: predicate,
                pageIndex: 0,
                pageSize: count,
                orderBy: list => list.OrderBy(y => y.CategoryName));

            var result = await FetchSubCategory();
            if (result.IsFailure)
            {
                return Result.Failure<List<CategoryDto>>(result.Error);
            }

            List<SubCategoryDto> subcategories = result.Value;
            response = categories_result.Items.Select(x =>
            new CategoryDto()
            {
                Id = x.CategoryId,
                IsActive = x.IsActive,
                Name = x.CategoryName,
                SubCategories = subcategories.Where(y => y.ParentCategoryId == x.CategoryId).ToList()
            }).ToList();

            return Result.Success(response);
        }
        public async Task<Result<List<SubCategoryDto>>> FetchSubCategory()
        {
            Expression<Func<CategoryMaster, bool>> predicate = x => x.IsActive && x.ParentCategoryId != null;
            int count = _repositoryCategoryMaster.Count(predicate: predicate);
            IPagedList<CategoryMaster> subcategories_result = await _repositoryCategoryMaster
                .GetPagedListAsync(
                predicate: predicate,
                pageIndex: 0,
                pageSize: count,
                orderBy: list => list.OrderBy(y => y.CategoryName));
            List<SubCategoryDto> response = subcategories_result.Items.Select(x =>
            new SubCategoryDto()
            {
                Id = x.CategoryId,
                IsActive = x.IsActive,
                Name = x.CategoryName,
                ParentCategoryId = x.ParentCategoryId.Value
            }).ToList();
            return Result.Success(response);
        }
        public async Task<Result<int>> CreateCategoryOrSubCategory(SubCategoryDto request)
        {
            bool isPermitted = predicateBuilder.CanCreateCategoryOrSubCategory();
            if (!isPermitted)
            {
                return Result.Failure<int>(DomainErrors.Authorization.UnauthorizedAccess);
            }

            if (request == null)
                return Result.Failure<int>(DomainErrors.Common.NullOrEmptyValue(request.Name));

            CategoryMaster categoryMaster = new()
            {
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = _authenticatedUserService.AuthUser.Id,
                CategoryName = request.Name,
            };
            if (request.ParentCategoryId == 0)
            {
                categoryMaster.ParentCategoryId = null;
            }
            else
            {
                var result = await FetchCategory();

                if (result.IsFailure)
                {
                    return Result.Failure<int>(result.Error);
                }

                var parent = result.Value.FirstOrDefault(x => x.Id == request.ParentCategoryId);

                if (parent == null)
                {
                    return Result.Failure<int>(DomainErrors.Common.NullOrEmptyValue(request.ParentCategoryId.ToString()));
                }

                categoryMaster.ParentCategoryId = request.ParentCategoryId;
            }
            await _repositoryCategoryMaster.InsertAsync(categoryMaster);
            int response = await _unitOfWork.SaveChangesAsync();
            return Result.Success(response);
        }


        public async Task<List<TrainingDto>> FetchTraining()
        {
            Expression<Func<TrainingMaster, bool>> predicate = x => x.IsActive;
            int count = _repositoryTrainingMaster.Count(predicate: predicate);
            IPagedList<TrainingMaster> Training_result = await _repositoryTrainingMaster
                .GetPagedListAsync(
                predicate: predicate,
                pageIndex: 0,
                pageSize: count,
                orderBy: list => list.OrderBy(y => y.TrainingName));
            List<TrainingDto> response = Training_result.Items.Select(x =>
            new TrainingDto()
            {
                TrainingId = x.TrainingId,
                TrainingName = x.TrainingName

            }).ToList();
            return response;
        }

        public async Task<IPagedList<TrainingDto>> FetchPagedTrainingList(string FilterByName, int? pageIndex, int? pageSize)
        {
            Expression<Func<TrainingMaster, bool>> predicate =  FilterByName.IsNullOrEmpty() ?  x => x.IsActive : 
                x => x.IsActive && (x.TrainingName.ToLower().Contains(FilterByName.ToLower()) || x.TrainingName.ToLower() == FilterByName.ToLower());
            IPagedList<TrainingDto> Training_result = await _repositoryTrainingMaster
                .GetPagedListAsync(
                predicate: predicate,
                pageIndex: pageIndex ?? 0,
                pageSize: pageSize ?? 0,
                orderBy: list => list.OrderBy(y => y.TrainingName),
                selector: x => new TrainingDto
                {
                    TrainingId = x.TrainingId,
                    TrainingName = x.TrainingName,
                    IsPriortize = x.IsPriortize,
                });

            return Training_result;

        }
        public async Task<List<TrainingStatusListDto>> FetchTrainingStatus()
        {
            var statusList = EnumHelper.EnumToKeyValueList<TrainingStatus>();
            List<TrainingStatusListDto> response = statusList.Select(x =>
            new TrainingStatusListDto()
            {
                TrainingStatusId = x.Key,
                TrainingStatusName = x.Value
            }).ToList();
            return response;
        }
        public async Task<List<ReportColumnConfigurationDto>> FetchReportSelectColumns(string ActivityType)
        {
            string activityTypeLower = ActivityType?.ToLower() ?? "";
            var response = await _academyDbContext.ReportColumnConfigurations
                .Where(x =>
                    !x.ReportColumnName.ToLower().Contains("activitymaster.activityname") &&
                    !x.ReportColumnName.ToLower().Contains("trainingmaster.trainingname") ||
                    (activityTypeLower == "activity" && x.ReportColumnName.ToLower().Contains("activitymaster.activityname")) ||
                    (activityTypeLower == "training" && x.ReportColumnName.ToLower().Contains("trainingmaster.trainingname"))
                )
                .Select(x => new ReportColumnConfigurationDto
                {
                    ReportColumnConfigId = x.ReportColumnConfigId,
                    ReportColumnName = x.ReportColumnName,
                    ReportColumnDisplayName =
                        x.ReportColumnDisplayName.ToLower() == "tdc"
                            ? "Country"
                            : (activityTypeLower == "activity" && x.ReportColumnName.ToLower().Contains("activitymaster.activityname"))
                                ? "Activity"
                                : (activityTypeLower == "training" && x.ReportColumnName.ToLower().Contains("trainingmaster.trainingname"))
                                    ? "Training"
                                    : x.ReportColumnDisplayName,
                    IsGroupBy = x.IsGroupBy
                })
                .ToListAsync();
            return response;
        }
        public async Task<List<ReportColumnConfigurationDto>> FetchReportGroupByColumns(string ActivityType)
        {
            string activityTypeLower = ActivityType?.ToLower() ?? "";
            List<ReportColumnConfigurationDto> response = _academyDbContext.ReportColumnConfigurations.Where(x => x.IsGroupBy == true &&
                    !x.ReportColumnName.ToLower().Contains("activitymaster.activityname") &&
                    !x.ReportColumnName.ToLower().Contains("trainingmaster.trainingname") ||
                    (activityTypeLower == "activity" && x.ReportColumnName.ToLower().Contains("activitymaster.activityname")) ||
                    (activityTypeLower == "training" && x.ReportColumnName.ToLower().Contains("trainingmaster.trainingname"))).Select(x =>
            new ReportColumnConfigurationDto()
            {
                ReportColumnConfigId = x.ReportColumnConfigId,
                ReportColumnName = x.ReportColumnName,
                ReportColumnDisplayName =
                        x.ReportColumnDisplayName.ToLower() == "tdc"
                            ? "Country"
                            : (activityTypeLower == "activity" && x.ReportColumnName.ToLower().Contains("activitymaster.activityname"))
                                ? "Activity"
                                : (activityTypeLower == "training" && x.ReportColumnName.ToLower().Contains("trainingmaster.trainingname"))
                                    ? "Training"
                                    : x.ReportColumnDisplayName,
                IsGroupBy = x.IsGroupBy

            }).ToList();
            return response;
        }
        public async Task<List<ReportTypeDto>> FetchReportType()
        {
            List<ReportTypeDto> response = _academyDbContext.ReportTypes.Where(x => x.IsActive == true).Select(x =>
            new ReportTypeDto()
            {
                ReportId = int.Parse(x.ReportId.ToString()),
                ReportName = x.ReportName,
                StoredProcName = x.StoredProcName
            }).ToList();
            return response;
        }
        public async Task<List<TrainingDto>> FetchTrainingByCommunity(string[] Communities)
        {
            var Training_result = await (from tm in _academyDbContext.TrainingMasters
                                         join etm in _academyDbContext.EmployeeTrainingMaps on tm.TrainingId equals etm.TrainingId
                                         join e in _academyDbContext.Employees on etm.EmployeeId equals e.Id
                                         where e.IsActive == true && Communities.Contains(e.Community)
                                         select new { tm.TrainingId, tm.TrainingName })
                            .Distinct()
                            .ToListAsync();

            List<TrainingDto> response = Training_result.Select(x =>
            new TrainingDto()
            {
                TrainingId = x.TrainingId,
                TrainingName = x.TrainingName

            }).ToList();

            return response;
        }
        public async Task<List<ActivityMasterDto>> FetchPrimaryActivityByCommunity(string[] Communities)
        {
            var Training_result = (from am in _academyDbContext.ActivityMasters
                                   join eam in _academyDbContext.EmployeeActivityMaps on am.ActivityId equals eam.ActivityId
                                   join e in _academyDbContext.Employees on eam.EmployeeId equals e.Id
                                   where e.IsActive == true && Communities.Contains(e.Community)
                                   select new { am.ActivityId, am.ActivityName })
                            .Distinct()
                            .ToList();
            List<ActivityMasterDto> response = Training_result.Select(x =>
            new ActivityMasterDto()
            {
                ActivityId = x.ActivityId,
                ActivityName = x.ActivityName
            }).ToList();
            return response;
        }

        public async Task<List<ActivityMasterDto>> FetchAllPrimaryActivity()
        {
            var Training_result = (from am in _academyDbContext.ActivityMasters
                                   join eam in _academyDbContext.EmployeeActivityMaps on am.ActivityId equals eam.ActivityId
                                   join e in _academyDbContext.Employees on eam.EmployeeId equals e.Id
                                   where e.IsActive == true
                                   select new { am.ActivityId, am.ActivityName })
                            .Distinct()
                            .ToList();
            List<ActivityMasterDto> response = Training_result.Select(x =>
            new ActivityMasterDto()
            {
                ActivityId = x.ActivityId,
                ActivityName = x.ActivityName
            }).ToList();
            return response;
        }

        public async Task<List<PrimaryActivityTypeDto>> FetchPrimaryActivity()
        {
            var statusList = EnumHelper.EnumToKeyValueList<PrimaryActivityType>();
            List<PrimaryActivityTypeDto> response = statusList.Select(x =>
            new PrimaryActivityTypeDto()
            {
                PrimaryActivityId = x.Key,
                PrimaryActivityName = x.Value
            }).ToList();
            return response;
        }

      

        public async Task<List<TrainingDto>> FetchByAreaPathAndCommunity(string[] Communities, string[] Areapaths)
        {
            var Training_result = await (
                              from tm in _academyDbContext.TrainingMasters
                              join etm in _academyDbContext.EmployeeTrainingMaps on tm.TrainingId equals etm.TrainingId
                              join e in _academyDbContext.Employees on etm.EmployeeId equals e.Id
                              join lp in _academyDbContext.LearningPathTrainingMaps on etm.TrainingId equals lp.TrainingId
                              join l in _academyDbContext.LearningPaths on lp.LearningPathId equals l.LearningPathId
                              where l.IsActive
                                    && Communities.Contains(e.Community)
                                    && Areapaths.Contains(l.LearningPathId.ToString())   // ✅ Use AreaPath column instead of LearningPathId
                              select new
                              {
                                  tm.TrainingId,
                                  tm.TrainingName
                              })
                              .Distinct()
                              .ToListAsync();

            List<TrainingDto> response = Training_result.Select(x =>
            new TrainingDto()
            {
                TrainingId = x.TrainingId,
                TrainingName = x.TrainingName

            }).ToList();

            return response;
        }

        public async Task<List<TrainingDto>> FetchByAreaPath(string[] Areapaths)
        {
            var Training_result = await (
                              from tm in _academyDbContext.TrainingMasters
                              join etm in _academyDbContext.EmployeeTrainingMaps on tm.TrainingId equals etm.TrainingId
                              join e in _academyDbContext.Employees on etm.EmployeeId equals e.Id
                              join lp in _academyDbContext.LearningPathTrainingMaps on etm.TrainingId equals lp.TrainingId
                              join l in _academyDbContext.LearningPaths on lp.LearningPathId equals l.LearningPathId
                              where l.IsActive
                                    && Areapaths.Contains(l.LearningPathId.ToString())   // ✅ Use AreaPath column instead of LearningPathId
                              select new
                              {
                                  tm.TrainingId,
                                  tm.TrainingName
                              })
                              .Distinct()
                              .ToListAsync();

            List<TrainingDto> response = Training_result.Select(x =>
            new TrainingDto()
            {
                TrainingId = x.TrainingId,
                TrainingName = x.TrainingName

            }).ToList();

            return response;
        }
        public async Task<List<TrainingsGroupedBySkill>> FetchAISkillTrainingsMetaData()
        {
            Dictionary<string, object> iParam = new();
            var table = await _adoClient.ExecuteReaderAsync(DbConstants.FETCH_AI_SKILL_TRAININGS_METADATA, iParam);
            if (table != null)
            {
                List<SkillTrainingDto> result = table.ToList<SkillTrainingDto>();
                if (result != null)
                {
                    List<TrainingsGroupedBySkill> response = new();
                    foreach (var item in result)
                    {
                        if (!response.Any(x => x.SkillId == item.SkillId))
                        {
                            TrainingsGroupedBySkill parent = new()
                            {
                                SkillId = item.SkillId,
                                SkillName = item.SkillName,
                                EcosystemId = item.EcosystemId,
                                ExpectedProficiency = item.ExpectedProficiency,
                                ExpectedKnowledge = item.ExpectedKnowledge
                            };
                            response.Add(parent);
                        }
                        response.FirstOrDefault(x => x.SkillId.Equals(item.SkillId)).Trainings.Add(
                            new TrainingMasterResponse()
                            {
                                IsMvP = item.IsMvP,
                                TrainingId = item.TrainingId,
                                SeniorityId = item.SeniorityId,
                                Seniority = ApplicationConstants.ALLOWED_SENIORITIES_DETAILS.FirstOrDefault(x => x.Key.Value == item.SeniorityId).Value,
                                TrainingName = item.TrainingName,
                            }
                        );
                    }
                    return response;
                }
            }
            return new();
        }
    }
}