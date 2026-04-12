using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Academy.Shared.Extensions;
using Academy.Shared.Response;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Position = Academy.Shared.Enums.Position;
using EmpValue = Academy.Shared.Exceptions.DomainErrors.Employee;

namespace Academy.Core.Services
{
    public class GXLeaderService : IGXLeaderService
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IPredicateFactory _predicateFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Employee> _repositoryEmployee;
        private readonly IRepository<ProposedDojoGxLeader> _repositoryProposedDojoGxLeader;
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IEmployeeService _employeeService;
        private readonly IRepository<DojoDetail> _repositoryDojoDetail;
        private readonly IRepository<DojoGxLeaderAssignment> _repositoryDojoGxLeaderAssignment;

        public GXLeaderService(
            IAuthenticatedUserService authenticatedUserService,
            IUnitOfWork unitOfWork,
            IPredicateFactory predicateFactory,
            IAcademyDbContext academyDbContext,
            IEmployeeService employeeService

            )
        {
            _unitOfWork = unitOfWork;
            _authenticatedUserService = authenticatedUserService;
            _predicateFactory = predicateFactory;
            _repositoryEmployee = _unitOfWork.GetRepository<Employee>();
            _repositoryProposedDojoGxLeader = _unitOfWork.GetRepository<ProposedDojoGxLeader>();
            _academyDbContext = academyDbContext;
            _employeeService = employeeService;
            _repositoryDojoDetail = _unitOfWork.GetRepository<DojoDetail>();
            _repositoryDojoGxLeaderAssignment = _unitOfWork.GetRepository<DojoGxLeaderAssignment>();
        }

        public async Task<Result<List<LeaderModel>>> GetGXAllLeader(string community)
        {
            // Build base predicate (role-based filter)
            var predicateBuilder = _predicateFactory.PredicateGenerator(_authenticatedUserService.AuthUser.Roles);
            Expression<Func<Employee, bool>> basePredicate = predicateBuilder.FetchEmployees();

            // Seniority filter for leaders
            var seniorityIds = new[] {
                                        (int)Seniority.TechManager,
                                        (int)Seniority.SubjectMatterExpert,
                                        (int)Seniority.Architect,
                                        (int)Seniority.SrLevel3,
                                        (int)Seniority.SoftwareDesigner,
                                        (int)Seniority.SrLevel2

                                    };
            // Get IQueryable of all employees from repository (do filtering server-side where possible)
            var empQuery = _repositoryEmployee.GetAll(); // assumed IQueryable<Employee>
            var dojoProjects = _academyDbContext.DojoProjectConfigurations.Where(dpc => dpc.IsAssignable).Select(d => d.ProjectName.ToLower()).ToList();
            // Combine filters in one go
            var empQueryWithCommunity = empQuery.Where(e => e.IsActive && dojoProjects.Contains(e.Project.ToLower())); //&& e.Tdc == "India"

            var positions = Enum.GetValues<Position>()
                         .Select(p => p.GetDisplayName())
                         .ToList();

            var empQueryForPosition = empQuery.Where(e => e.IsActive && positions.Contains(e.Position) && dojoProjects.Contains(e.Project.ToLower()) ); //&& e.Tdc == "India"
            // Combine with base predicate if available
            if (basePredicate != null)
            {
                empQueryWithCommunity = empQueryWithCommunity.Where(basePredicate);
            }

            // ====== Fetch leaders (no pagination) ======
            var allLeaders = empQueryWithCommunity
                .Where(e => e.SeniorityId.HasValue && seniorityIds.Contains(e.SeniorityId.Value))
                .Select(e => new LeaderModel
                {
                    Id = e.Id,
                    EmployeeName = e.EmployeeName,
                    GlobantEmailAddress = e.GlobantEmailAddress,
                    Seniority = e.Seniority,
                    Client = e.Client,
                    Project = e.Project,
                    ProposedLeaderEmail = e.ProposedLeaderEmail,
                    BetterMeLeaderEmail = e.BetterMeLeaderEmail,
                    GexLeaders = e.GexLeaders,
                    SeniorityName = e.Seniority,
                    SeniorityId = e.SeniorityId ?? 0,
                    MinMentee = 0,
                    MaxMentee = 30,
                    IsLeader = !string.IsNullOrEmpty(e.BetterMeLeaderEmail),
                    CommunityName = e.Community,
                    LeaderAssignDate = e.JoiningDate.HasValue ? e.JoiningDate.Value.ToString() : "",
                    InOutDate = e.JoiningDate,
                    InOut = true,
                    tdc = e.Tdc
                })
                .ToList(); // execute query

            var LeaderWithPosition = empQueryForPosition.Select(e => new LeaderModel
            {
                Id = e.Id,
                EmployeeName = e.EmployeeName,
                GlobantEmailAddress = e.GlobantEmailAddress,
                Seniority = e.Seniority,
                Client = e.Client,
                Project = e.Project,
                ProposedLeaderEmail = e.ProposedLeaderEmail,
                BetterMeLeaderEmail = e.BetterMeLeaderEmail,
                GexLeaders = e.GexLeaders,
                SeniorityName = e.Seniority,
                SeniorityId = e.SeniorityId ?? 0,
                MinMentee = 0,
                MaxMentee = 30,
                IsLeader = !string.IsNullOrEmpty(e.BetterMeLeaderEmail),
                CommunityName = e.Community,
                LeaderAssignDate = e.JoiningDate.HasValue ? e.JoiningDate.Value.ToString() : "",
                InOutDate = e.JoiningDate,
                InOut = true,
                tdc = e.Tdc
            })
        .ToList();
            LeaderWithPosition.AddRange(allLeaders);

            // Get mentee counts from ProposedDojoGxLeader table
            var gxLeaderQuery = _repositoryProposedDojoGxLeader.GetAll(); // IQueryable<ProposedDojoGxLeader>

            gxLeaderQuery = gxLeaderQuery.Where(x => x.IsActive)
                                         .Where(x => !string.IsNullOrEmpty(x.ProposedDojoLeaderEmailId));

            var distinctLeader = empQueryWithCommunity.Union(empQueryForPosition);

            // optional: if community filter applies via Employee.Community, join and filter
            if (!string.IsNullOrEmpty(community))
            {
                var lowerCommunity = community.Trim().ToLower();
                gxLeaderQuery = from g in gxLeaderQuery
                                join e in distinctLeader on g.ProposedDojoLeaderEmailId equals e.GlobantEmailAddress
                                select g;
            }

            var menteeCounts = await gxLeaderQuery.GroupBy(x => x.ProposedDojoLeaderEmailId)
                                                  .Select(g => new
                                                  {
                                                      LeaderEmail = g.Key,
                                                      MenteesCount = g.Count()
                                                  })
                                                  .ToDictionaryAsync(x => x.LeaderEmail, x => x.MenteesCount);

            // Merge mentee counts into leaders
            var leadersWithCounts = LeaderWithPosition.Distinct().Select(l =>
                                                {
                                                    menteeCounts.TryGetValue(l.GlobantEmailAddress ?? "", out int count);
                                                    l.MenteesCount = count;
                                                    return l;
                                                })
                                                .ToList();

            return Result.Success(leadersWithCounts);
        }

        /// <summary>
        /// Delete GX Leader
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteGXLeader(UpdateGxLeader request)
        {
            //Get Employee details 
            var employee = await _employeeService.FetchById(request.EmployeeId);
            if (employee.IsFailure)
                return Result.Failure<int>(employee.Error);

            //Get DOJO details
            var dojoDetails = await _repositoryDojoDetail.GetFirstOrDefaultAsync(predicate: x => x.EmployeeId == request.EmployeeId);
            if (dojoDetails == null)
                return Result.Failure<int>(EmpValue.NotFound);

            //Remove DojoDetails by just removing the DojoGexLeaderEmail
            dojoDetails.DojoGexLeaderEmail = string.Empty;
            dojoDetails.UpdatedOn = DateTime.UtcNow;
            dojoDetails.UpdatedBy = _authenticatedUserService.AuthUser.Id;
            dojoDetails.DojoEndDate = DateTime.UtcNow;
            dojoDetails.IsActive = false;

            _repositoryDojoDetail.Update(dojoDetails);

            //TODO: Call DeleteGxLeaderAssignmentAsync 
            var removeGxLeaderAssignmentTask = await DeleteGxLeaderAssignmentAsync(dojoDetails.DojoDetailId, request);

            if (!removeGxLeaderAssignmentTask)
            {
                return Result.Failure<int>(EmpValue.RemoveGXLeader);
            }

            //Call DeleteProposedDojoGxLeaders
            await DeleteProposedDojoGxLeadersAsync(request);

            int count = await _unitOfWork.SaveChangesAsync();
            return count;
        }

        //TODO: Review needs to be done
        public async Task<bool> DeleteGxLeaderAssignmentAsync(int dojoDetailId, UpdateGxLeader request)
        {
            bool isDojoGexLeaderRemoved = false;

            var dojoGxLeaderAssignmentDetails = await _academyDbContext.DojoGxLeaderAssignments.Where(x => x.DojoDetailId == dojoDetailId && x.IsActive == true).ToListAsync();

            if (dojoGxLeaderAssignmentDetails.Count >= 0)
            {
                foreach (var item in dojoGxLeaderAssignmentDetails)
                {
                    item.IsActive = false;
                    item.AssignmentEndDate = DateTime.UtcNow;
                    item.UpdatedOn = DateTime.UtcNow;
                    item.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                }

                isDojoGexLeaderRemoved = true;

                _repositoryDojoGxLeaderAssignment.Update(dojoGxLeaderAssignmentDetails);
            }

            return isDojoGexLeaderRemoved;
        }

        //TODO: Need to discuss 
        public async Task DeleteProposedDojoGxLeadersAsync(UpdateGxLeader request)
        {
            var proposedDojoGxLeader = await _academyDbContext.ProposedDojoGxLeaders.Where(x => x.EmployeeId == request.EmployeeId).FirstOrDefaultAsync();

            if (proposedDojoGxLeader != null)
            {
                //should we update the record with empty values
                proposedDojoGxLeader.ProposedDojoLeaderEmailId = string.Empty;
                proposedDojoGxLeader.ProposedLeaderName = string.Empty;
                proposedDojoGxLeader.ProposedLeaderSeniority = string.Empty;
                proposedDojoGxLeader.UpdatedOn = DateTime.UtcNow;
                proposedDojoGxLeader.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                proposedDojoGxLeader.IsActive = false;
                _repositoryProposedDojoGxLeader.Update(proposedDojoGxLeader);

            }
        }
    }
}