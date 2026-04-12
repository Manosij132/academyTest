using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Core.PredicateBuilder;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using static Academy.Shared.Exceptions.DomainErrors;
using Employee = Academy.Shared.Exceptions.DomainErrors.Employee;

namespace Academy.Core.Services
{
    public class DojoService : IDojoService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IPredicateFactory _predicateFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdoClient<AcademyDbSetting> _academyDbAdoClient;
        private readonly IRepository<DojoDetail> _repositoryDojoDetail;
        private readonly IEmployeeService _employeeService;
        private readonly IAcademyDbContext _academyDbContext;
        private readonly AbstractAdminPredicate predicateBuilder;
        private readonly IRepository<DojoGxLeaderAssignment> _repositoryDojoGxLeaderAssignment;
        private readonly IRepository<EmailDump> _repositoryEmail;
        private readonly IRepository<ProposedDojoGxLeader> _repositoryProposedDojoGxLeader;

        public DojoService(
            IAuthenticatedUserService authenticatedUserService,
            IUnitOfWork unitOfWork,
            IEmployeeService employeeService,
            IOptions<AppSetting> appSetting,
            IAcademyDbContext academyDbContext,
            IAdoClient<AcademyDbSetting> academyDbAdoClient,
            IPredicateFactory predicateFactory,
            IGoogleApiManager googleApiManager,
            ISkillAndTrainingService skillAndTrainingService
            )
        {
            _unitOfWork = unitOfWork;
            _authenticatedUserService = authenticatedUserService;
            _academyDbAdoClient = academyDbAdoClient;
            _predicateFactory = predicateFactory;
            _repositoryDojoDetail = _unitOfWork.GetRepository<DojoDetail>();
            _employeeService = employeeService;
            _academyDbContext = academyDbContext;
            predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            _repositoryDojoGxLeaderAssignment = _unitOfWork.GetRepository<DojoGxLeaderAssignment>();
            _repositoryEmail = _unitOfWork.GetRepository<EmailDump>();
            _repositoryProposedDojoGxLeader = _unitOfWork.GetRepository<ProposedDojoGxLeader>();
        }

        public async Task<Result<int>> UpdateDojoDetailTrainingInfo(List<UpdateDojoDetailTrainingInfo> list)
        {
            // Avoid concurrent DbContext operations by processing sequentially
            int updatedCount = 0;
            foreach (var item in list)
            {
                var entity = await _repositoryDojoDetail.GetFirstOrDefaultAsync(predicate: x => x.DojoDetailId.Equals(item.DojoDetailId));
                if (entity != null)
                {
                    entity.AssignedThroughTraining = item.AssignedThroughTraining;
                    entity.Comments = item.Comments;
                    entity.TicketNumber = item.TicketNumber > 0 ? item.TicketNumber : null;
                    entity.UpdatedOn = DateTime.Now;
                    entity.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                    _repositoryDojoDetail.Update(entity);
                    updatedCount++;
                }
            }

            var affectedRows = await _unitOfWork.SaveChangesAsync();
            return Result.Success(affectedRows);
        }

        public async Task<Result<int>> UpdateDojoEndtDate(List<UpdateDojoEndDate> list)
        {
            // Avoid concurrent DbContext operations by processing sequentially
            int updatedCount = 0;
            foreach (var item in list)
            {
                var entity = await _repositoryDojoDetail.GetFirstOrDefaultAsync(predicate: x => x.DojoDetailId.Equals(item.DojoDetailId));
                if (entity != null)
                {
                    entity.DojoEndDate = item.DojoEndDate;
                    entity.UpdatedOn = DateTime.Now;
                    entity.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                    _repositoryDojoDetail.Update(entity);
                    updatedCount++;
                }
            }

            var affectedRows = await _unitOfWork.SaveChangesAsync();
            return Result.Success(affectedRows);
        }

        public async Task<Result<GetDojoDetailsResponse>> GetFilteredPagedDojoDetails(FetchDojoGlobarsRequest request)
        {
            GetDojoDetailsResponse dojoDetails = new();

            var predicateBuilder = _predicateFactory
               .PredicateGenerator(_authenticatedUserService.AuthUser.Roles);

            bool isPermitted = predicateBuilder.CanGetFilteredPagedDojoDetails();
            if (!isPermitted)
            {
                return Result.Failure<GetDojoDetailsResponse>(Authorization.UnauthorizedAccess);
            }

            Dictionary<string, object> iParams = new()
            {
                { "@SearchTerm", request.SearchText },
                { "@PageNumber", request.PageIndex },
                { "@PageSize", request.PageSize },
            };

            if (request.Community != null && request.Community.Any())
            {
                var communities = string.Join(",", request.Community);
                iParams.Add("@Community", communities);
            }
            if (request.Country != null && request.Country.Any())
            {
                var countries = string.Join(",", request.Country);
                iParams.Add("@Country", countries);
            }
            if (request.AiStudio != null && request.AiStudio.Any())
            {
                var aiStudios = string.Join(",", request.AiStudio);
                iParams.Add("@AiStudio", aiStudios);
            }
            if (request.Account != null && request.Account.Any())
            {
                var accounts = string.Join(",", request.Account);
                iParams.Add("@Account", accounts);
            }

            var dataset = await _academyDbAdoClient.XecuteReaderDataSetAsync("usp_GetDojoGlobarDetails", iParams);

            if (dataset.Tables.Count > 0)
            {
                var reader = dataset.Tables[0];

                foreach (DataRow row in reader.Rows)
                {
                    dojoDetails.Items.Add(new DojoDetailInfo
                    {
                        DojoDetailId = row["DojoDetailId"] == DBNull.Value ? 0 : Convert.ToInt32(row["DojoDetailId"]),
                        EmployeeId = row["EmployeeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["EmployeeId"]),
                        EmployeeName = row["EmployeeName"] == DBNull.Value ? string.Empty : Convert.ToString(row["EmployeeName"]),
                        AiStudio = row["AiStudio"] == DBNull.Value ? string.Empty : Convert.ToString(row["AiStudio"]),
                        Account = row["Account"] == DBNull.Value ? string.Empty : Convert.ToString(row["Account"]),
                        GlobantEmailAddress = row["GlobantEmailAddress"] == DBNull.Value ? string.Empty : Convert.ToString(row["GlobantEmailAddress"]),
                        DojoStartDate = row["DojoStartDate"] == DBNull.Value ? null : Convert.ToDateTime(row["DojoStartDate"]),
                        DojoEndDate = row["DojoEndDate"] == DBNull.Value ? null : Convert.ToDateTime(row["DojoEndDate"]),

                        //DojoStartDate = row["DojoStartDate"] == DBNull.Value ? null : Convert.ToDateTime(row["DojoStartDate"]).ToString("dd-MMM-yyyy"),
                        //DojoEndDate = row["DojoEndDate"] == DBNull.Value ? null : Convert.ToDateTime(row["DojoEndDate"]).ToString("dd-MMM-yyyy"),

                        DojoGexLeaderEmail = row["DojoGexLeaderEmail"] == DBNull.Value ? string.Empty : Convert.ToString(row["DojoGexLeaderEmail"]),
                        AssignedThroughTraining = row["AssignedThroughTraining"] == DBNull.Value ? null : Convert.ToBoolean(row["AssignedThroughTraining"]),
                        Comments = row["Comments"] == DBNull.Value ? string.Empty : Convert.ToString(row["Comments"]),
                        TicketNumber = row["TicketNumber"] == DBNull.Value ? null : Convert.ToInt32(row["TicketNumber"]),
                        Community = row["Community"] == DBNull.Value ? string.Empty : Convert.ToString(row["Community"]),
                    });
                }
                var reader2 = dataset.Tables[1];
                if (reader2.Rows.Count > 0)
                {
                    dojoDetails.TotalCount = reader2.Rows[0]["TotalFilteredRecords"] == DBNull.Value ? 0 : Convert.ToInt32(reader2.Rows[0]["TotalFilteredRecords"]);
                }
                else
                    dojoDetails.TotalCount = 0;

                dojoDetails.PageSize = request.PageSize;
                dojoDetails.PageIndex = request.PageIndex;
                dojoDetails.TotalPages = (int)Math.Ceiling((double)dojoDetails.TotalCount / dojoDetails.PageSize);

            }
            return Result.Success(dojoDetails);
        }

        public async Task<Result<int>> UpdateGXLeader(UpdateGxLeader request)
        {
            bool isPermitted = predicateBuilder.CanUpdateDojoGxLeadxer();
            if (!isPermitted)
            {
                return Result.Failure<int>(Authorization.UnauthorizedAccess);
            }

            var employee = await _employeeService.FetchById(request.EmployeeId);
            if (employee.IsFailure)
                return Result.Failure<int>(employee.Error);

            var dojoDetails = await _repositoryDojoDetail.GetFirstOrDefaultAsync(predicate: x => x.EmployeeId == request.EmployeeId);
            if (dojoDetails == null)
                return Result.Failure<int>(Employee.NotFound);

            //If UpdatedGXLeader AlreadyExists then return it
            if (String.Equals(dojoDetails.DojoGexLeaderEmail, request.DojoGxLeaderEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<int>(Employee.GXLeaderAlreadyExists);
            }

            //Update DojoDetails
            dojoDetails.DojoGexLeaderEmail = request.DojoGxLeaderEmail;
            dojoDetails.UpdatedOn = DateTime.UtcNow;
            dojoDetails.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            _repositoryDojoDetail.Update(dojoDetails);

            //Call UpdateGxLeaderAssignment
            var updateGxLeaderAssignmentTask = await UpdateGxLeaderAssignmentAsync(dojoDetails.DojoDetailId, request);

            if (!updateGxLeaderAssignmentTask)
            {
                return Result.Failure<int>(Employee.MultipleGXLeaderExists);
            }

            //Call UpdateProposedDojoGxLeaders
            await UpdateProposedDojoGxLeadersAsync(request);

            var isNewMailSent = await _academyDbContext.DojoGxLeaderAssignments.Where(x => x.DojoDetailId == dojoDetails.DojoDetailId).OrderByDescending(s => s.UpdatedOn).FirstOrDefaultAsync();
            if (isNewMailSent == null || (isNewMailSent != null && isNewMailSent.IsActive))
            {
                //Mail
                var dojoLeaderEmailDump = new EmailDump
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Subject = "New DOJO GX Leader Assignment",
                    Template = "DOJOGX_LEADER",
                    Cc = request.DojoGxGlobarEmail,
                    To = request.DojoGxLeaderEmail,
                    IsActive = true
                };
                _repositoryEmail.Insert(dojoLeaderEmailDump);

                // add entries into email dump
                var globarEmailDump = new EmailDump
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Subject = "DOJO GX Leader Change Notification",
                    Template = "DOJOGX_GLOBER",
                    Cc = request.DojoGxLeaderEmail,
                    To = request.DojoGxGlobarEmail,
                    IsActive = true
                };
                _repositoryEmail.Insert(globarEmailDump);
            }
            else
            {
                var previousLeaderDetails = await _academyDbContext.DojoGxLeaderAssignments.Where(x => x.DojoDetailId == dojoDetails.DojoDetailId && !x.IsActive).OrderByDescending(s => s.UpdatedOn).FirstOrDefaultAsync();
                //Mail
                var dojoLeaderUpdatedEmailDump = new EmailDump
                {
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Subject = "Updated DOJO GX Leader Change Notification",
                    Template = "DOJOGX_UPDATE_LEADER",
                    Cc = string.Join(",", (request.DojoGxLeaderEmail?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Concat(new[] { previousLeaderDetails.LeaderEmail })),
                    To = request.DojoGxGlobarEmail,
                    IsActive = true
                };
                _repositoryEmail.Insert(dojoLeaderUpdatedEmailDump);

            }
            int count = await _unitOfWork.SaveChangesAsync();
            return count;
        }



        public async Task<bool> UpdateGxLeaderAssignmentAsync(int dojoDetailId, UpdateGxLeader request)
        {
            bool isDojoGexLeaderChanged = false;

            var dojoGxLeaderAssignmentDetails = await _academyDbContext.DojoGxLeaderAssignments.Where(x => x.DojoDetailId == dojoDetailId && x.IsActive == true).ToListAsync();

            if (dojoGxLeaderAssignmentDetails.Count > 1)
            {
                return isDojoGexLeaderChanged;
            }

            if (dojoGxLeaderAssignmentDetails.Count <= 0)
            {
                // add new record
                await _repositoryDojoGxLeaderAssignment.InsertAsync(new DojoGxLeaderAssignment
                {
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    DojoDetailId = dojoDetailId,
                    AssignmentStartDate = DateTime.UtcNow,
                    AssignmentEndDate = null,
                    LeaderEmail = request.DojoGxLeaderEmail,
                    IsActive = true,
                });
                isDojoGexLeaderChanged = true;
            }
            else
            {
                // case 1
                foreach (var item in dojoGxLeaderAssignmentDetails)
                {
                    item.IsActive = false;
                    item.AssignmentEndDate = DateTime.UtcNow;
                    item.UpdatedOn = DateTime.UtcNow;
                    item.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                }

                // add new record
                await _repositoryDojoGxLeaderAssignment.InsertAsync(new DojoGxLeaderAssignment
                {
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    DojoDetailId = dojoDetailId,
                    AssignmentStartDate = DateTime.UtcNow,
                    AssignmentEndDate = null,
                    LeaderEmail = request.DojoGxLeaderEmail,
                    IsActive = true,
                });
                isDojoGexLeaderChanged = true;

                _repositoryDojoGxLeaderAssignment.Update(dojoGxLeaderAssignmentDetails);
            }

            return isDojoGexLeaderChanged;
        }

        public async Task UpdateProposedDojoGxLeadersAsync(UpdateGxLeader request)
        {
            var proposedDojoGxLeader = await _academyDbContext.ProposedDojoGxLeaders.Where(x => x.EmployeeId == request.EmployeeId).FirstOrDefaultAsync();

            if (proposedDojoGxLeader != null)
            {
                proposedDojoGxLeader.ProposedDojoLeaderEmailId = request.DojoGxLeaderEmail;
                proposedDojoGxLeader.ProposedLeaderName = request.ProposedLeaderName;
                proposedDojoGxLeader.ProposedLeaderSeniority = request.ProposedLeaderSeniority;
                proposedDojoGxLeader.UpdatedOn = DateTime.UtcNow;
                proposedDojoGxLeader.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                _repositoryProposedDojoGxLeader.Update(proposedDojoGxLeader);
            }
            else
            {
                // add new record
                await _repositoryProposedDojoGxLeader.InsertAsync(new ProposedDojoGxLeader
                {
                    EmployeeId = request.EmployeeId,
                    ProposedDojoLeaderEmailId = request.DojoGxLeaderEmail,
                    GloberName = request.GloberName,
                    ProposedLeaderName = request.ProposedLeaderName,
                    ProposedLeaderSeniority = request.ProposedLeaderSeniority,
                    GloberSeniority = request.GloberSeniority,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = _authenticatedUserService.AuthUser.Id
                });
            }
        } 
        public async Task<Result<List<DojoActivity>>> FetchDojoActivityByIds(List<string> employeeEmails)
        {
            var results = await (from x in _academyDbContext.DojoDetails
                                 join e in _academyDbContext.Employees on x.EmployeeId equals e.Id
                                 join p in _academyDbContext.Positions on x.TicketNumber equals p.SrNumber into positionGroup
                                 from p in positionGroup.DefaultIfEmpty()
                                 join ps in _academyDbContext.PositionSkills on p.PositionId equals ps.OpenPositionId into skillGroup
                                 where employeeEmails.Contains(e.GlobantEmailAddress)
                                 && !x.IsActive && x.AssignedThroughTraining == null
                                 select new
                                 {
                                     x.EmployeeId,
                                     x.DojoDetailId,
                                     x.DojoStartDate,
                                     e.EmployeeName,
                                     e.GlobantEmailAddress,
                                     x.Comments,
                                     x.TicketNumber,
                                     PositionTitle = p.PositionTitle,
                                     Client = p.Client,
                                     ProjectName = p.ProjectName,
                                     Skills = skillGroup.Select(s => s.SkillName).ToList()
                                 }).ToListAsync();

            var latestDojoActivities = results
                .GroupBy(d => d.EmployeeId)
                .Select(g => g.OrderByDescending(d => d.DojoStartDate).FirstOrDefault())
                .Select(d => new DojoActivity
                {
                    EmployeeId = d.EmployeeId,
                    DojoDetailId = d.DojoDetailId,
                    DojoStartDate = d.DojoStartDate,
                    EmployeeName = d.EmployeeName,
                    GlobantEmailAddress = d.GlobantEmailAddress,
                    Comments = d.Comments,
                    TicketNumber = d.TicketNumber,
                    PositionTitle = d.PositionTitle,
                    Client = d.Client,
                    ProjectName = d.ProjectName,
                    Skills = string.Join(", ", d.Skills)
                })
                .ToList();

            return Result.Success(latestDojoActivities);
        }

        public async Task<Result<List<int>>> GetMenteesByEmail(string GXLeaderEmail)
        {
            return Result.Success(await _academyDbContext.ProposedDojoGxLeaders.Where(x => x.ProposedDojoLeaderEmailId.Equals(GXLeaderEmail) && x.IsActive == true)
                                        .Select(x => x.EmployeeId)
                                        .ToListAsync());
        }

        public async Task<Result<int>> UpdateMentees(UpdateMentees request)
        {
            bool isPermitted = predicateBuilder.CanUpdateDojoGxLeadxer();
            if (!isPermitted)
            {
                return Result.Failure<int>(Authorization.UnauthorizedAccess);
            }

            var result = await _academyDbContext.Employees.Where(x => request.EmployeeId.Contains(x.Id)).ToListAsync();
            if (result.Count() != request.EmployeeId.Count())
                return Result.Failure<int>(Employee.NotFound);

            //var dojoDetails = await _repositoryDojoDetail.GetFirstOrDefaultAsync(predicate: x => request.EmployeeId.Contains(x.EmployeeId));

            var dojoDetailsWithEmail = await _academyDbContext.DojoDetails.Where(x => x.DojoGexLeaderEmail.Contains(request.DojoGxGlobarEmail)).ToListAsync();
            var dojoDetails = await _academyDbContext.DojoDetails.Where(x => request.EmployeeId.Contains(x.EmployeeId) && x.IsActive == true).ToListAsync();
            //if (dojoDetails.Count() != request.EmployeeId.Count())
            //    return Result.Failure<int>(Employee.NotFound);

            //If UpdatedGXLeader AlreadyExists then return it
            var matchGXLeader = dojoDetails.Where(x => x.DojoGexLeaderEmail.Equals(request.DojoGxLeaderEmail));
            //if (matchGXLeader.Count() >= 0)// (String.Equals(dojoDetails.DojoGexLeaderEmail, request.DojoGxLeaderEmail, StringComparison.OrdinalIgnoreCase))
            //{
            //    return Result.Failure<int>(Employee.GXLeaderAlreadyExists);
            //}

            //Update DojoDetails
            dojoDetails.ForEach(x =>
            {
                x.DojoGexLeaderEmail = request.DojoGxGlobarEmail;
                x.UpdatedOn = DateTime.UtcNow;
                x.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            });

            var notExisting = dojoDetailsWithEmail
                                .Where(x => !dojoDetails.Any(d => d.EmployeeId == x.EmployeeId))
                                .ToList();

            notExisting.ForEach(x =>
            {
                x.DojoGexLeaderEmail = null;
                x.UpdatedOn = DateTime.UtcNow;
                x.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            });
            //dojoDetails.AddRange(notExisting);
            var dojoDetailsList = dojoDetails;
            dojoDetailsList.AddRange(notExisting);
            _repositoryDojoDetail.Update(dojoDetailsList);

            //Call UpdateGxLeaderAssignment
            var updateGxLeaderAssignmentTask = await UpdateMenteesAssignmentAsync(dojoDetails, notExisting, request);

            if (!updateGxLeaderAssignmentTask)
            {
                return Result.Failure<int>(Employee.MultipleGXLeaderExists);
            }

            //Call UpdateProposedDojoGxLeaders
            await UpdateMenteesProposedDojoGxLeadersAsync(request);

            //Mail
            var dojoLeaderEmailDump = new EmailDump
            {
                CreatedBy = _authenticatedUserService.AuthUser.Id,
                CreatedOn = DateTime.UtcNow,
                Subject = "New DOJO GX Leader Assignment",
                Template = "DOJOGX_LEADER",
                Cc = request.DojoGxGlobarEmail,
                To = request.DojoGxGlobarEmail,
                IsActive = true
            };
            _repositoryEmail.Insert(dojoLeaderEmailDump);

            // add entries into email dump
            var globarEmailDump = new EmailDump
            {
                CreatedBy = _authenticatedUserService.AuthUser.Id,
                CreatedOn = DateTime.UtcNow,
                Subject = "DOJO GX Leader Change Notification",
                Template = "DOJOGX_GLOBER",
                Cc = request.DojoGxGlobarEmail,
                To = request.DojoGxGlobarEmail,
                IsActive = true
            };
            _repositoryEmail.Insert(globarEmailDump);

            int count = await _unitOfWork.SaveChangesAsync();
            return count;            

        }

        public async Task<bool> UpdateMenteesAssignmentAsync(List<DojoDetail> dojoDetails, List<DojoDetail> notExisting, UpdateMentees request)
        {
            bool isDojoGexLeaderChanged = false;
            List<int> dojoDetailIds = dojoDetails                                        
                                        .Select(x => x.DojoDetailId)
                                        .ToList();

            var dojoGxLeaderAssignmentDetails = await _academyDbContext.DojoGxLeaderAssignments.Where(x => dojoDetailIds.Contains(x.DojoDetailId) && x.IsActive == true).ToListAsync();

            var existingIds = dojoGxLeaderAssignmentDetails
                    .Select(x => x.DojoDetailId)
                    .Distinct()
                    .ToList();

            var notExistingIds = dojoDetailIds
                        .Except(existingIds)
                        .ToList();

            if (notExistingIds.Count > 0)
            {
                List<DojoGxLeaderAssignment> dojoGxLeaderAssignments = new List<DojoGxLeaderAssignment>();
                foreach (var item in notExistingIds)
                {
                    DojoGxLeaderAssignment dojoGxLeaderAssignment = new DojoGxLeaderAssignment();
                    dojoGxLeaderAssignment.CreatedOn = DateTime.UtcNow;
                    dojoGxLeaderAssignment.CreatedBy = _authenticatedUserService.AuthUser.Id;
                    dojoGxLeaderAssignment.DojoDetailId = item;
                    dojoGxLeaderAssignment.AssignmentStartDate = DateTime.UtcNow;
                    dojoGxLeaderAssignment.AssignmentEndDate = null;
                    dojoGxLeaderAssignment.LeaderEmail = request.DojoGxLeaderEmail;
                    dojoGxLeaderAssignment.IsActive = true;
                    dojoGxLeaderAssignments.Add(dojoGxLeaderAssignment);
                }
                // add new record
                await _repositoryDojoGxLeaderAssignment.InsertAsync(dojoGxLeaderAssignments);
                isDojoGexLeaderChanged = true;
            }
            if (existingIds.Count > 0)
            {
                // case 1
                foreach (var item in dojoGxLeaderAssignmentDetails)
                {
                    item.IsActive = true;
                    item.AssignmentEndDate = DateTime.UtcNow;
                    item.UpdatedOn = DateTime.UtcNow;
                    item.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                }

                isDojoGexLeaderChanged = true;

                //Delete Unselected Ids
                List<int> notexistsdojoDetailIds = notExisting.Select(x => x.DojoDetailId).ToList();

                var notExistingdojoGxLeaderAssignmentDetails = await _academyDbContext.DojoGxLeaderAssignments.Where(x => notexistsdojoDetailIds.Contains(x.DojoDetailId) && x.IsActive == true).ToListAsync();
                foreach (var item in notExistingdojoGxLeaderAssignmentDetails)
                {
                    item.IsActive = false;
                    item.AssignmentEndDate = DateTime.UtcNow;
                    item.UpdatedOn = DateTime.UtcNow;
                    item.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                }
                dojoGxLeaderAssignmentDetails.AddRange(notExistingdojoGxLeaderAssignmentDetails);
                _repositoryDojoGxLeaderAssignment.Update(dojoGxLeaderAssignmentDetails);
            }

            return isDojoGexLeaderChanged;
        }

        public async Task UpdateMenteesProposedDojoGxLeadersAsync(UpdateMentees request)
        {
            var proposedDojoGxLeader = await _academyDbContext.ProposedDojoGxLeaders.Where(x => request.EmployeeId.Contains(x.EmployeeId) && x.IsActive == true).ToListAsync();
            var proposedDojoGxLeaderByEmail = await _academyDbContext.ProposedDojoGxLeaders.Where(x => x.ProposedDojoLeaderEmailId.Equals(request.DojoGxGlobarEmail) && x.IsActive == true).ToListAsync();

            if (proposedDojoGxLeader != null && proposedDojoGxLeader.Count > 0)
            {
                proposedDojoGxLeader.ForEach(x =>
                {
                    x.ProposedDojoLeaderEmailId = request.DojoGxGlobarEmail;
                    x.ProposedLeaderName = request.ProposedLeaderName;
                    x.ProposedLeaderSeniority = request.ProposedLeaderSeniority;
                    x.UpdatedOn = DateTime.UtcNow;
                    x.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                });

                _repositoryProposedDojoGxLeader.Update(proposedDojoGxLeader);
            }

            var existingIdsByEmail = proposedDojoGxLeaderByEmail
                    .Select(x => x.EmployeeId)
                    .Distinct()
                    .ToList();

            var existingIds = proposedDojoGxLeader
                    .Select(x => x.EmployeeId)
                    .Distinct()
                    .ToList();

            var notExistingIdsByEmail = existingIdsByEmail
                            .Except(existingIds)
                            .ToList();

            if (notExistingIdsByEmail != null && notExistingIdsByEmail.Count > 0)
            {
                var list = proposedDojoGxLeaderByEmail.Where(x => notExistingIdsByEmail.Contains(x.EmployeeId)).ToList();
                list.ForEach(x =>
                {
                    //x.ProposedDojoLeaderEmailId = null;
                    x.IsActive = false;
                    x.UpdatedOn = DateTime.UtcNow;
                    x.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                });
                proposedDojoGxLeader.AddRange(list);
                _repositoryProposedDojoGxLeader.Update(list);
            }

            var notExistingIds = request.EmployeeId
                            .Except(existingIds)
                            .ToList();
            if (notExistingIds.Count > 0)
            {
                List<ProposedDojoGxLeader> proposedDojoGxLeaders = new List<ProposedDojoGxLeader>();
                foreach (var item in notExistingIds)
                {
                    ProposedDojoGxLeader proojoGxLeader = new ProposedDojoGxLeader();
                    proojoGxLeader.EmployeeId = item;
                    proojoGxLeader.ProposedDojoLeaderEmailId = request.DojoGxGlobarEmail;
                    proojoGxLeader.GloberName = _academyDbContext.Employees.Where(x => x.Id.Equals(item)).FirstOrDefault().EmployeeName;
                    proojoGxLeader.ProposedLeaderName = request.ProposedLeaderName;
                    proojoGxLeader.ProposedLeaderSeniority = request.ProposedLeaderSeniority;
                    proojoGxLeader.GloberSeniority = _academyDbContext.Employees.Where(x => x.Id.Equals(item)).FirstOrDefault().Seniority;
                    proojoGxLeader.IsActive = true;
                    proojoGxLeader.CreatedOn = DateTime.UtcNow;
                    proojoGxLeader.CreatedBy = _authenticatedUserService.AuthUser.Id;
                    proposedDojoGxLeaders.Add(proojoGxLeader);
                }
                // add new record
                await _repositoryProposedDojoGxLeader.InsertAsync(proposedDojoGxLeaders);
            }
        }
    }
}
