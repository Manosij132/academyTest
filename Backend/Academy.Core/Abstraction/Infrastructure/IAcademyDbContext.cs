using Academy.Domain.Entities;
using Academy.Domain.StoreProcedureEntities;
using Academy.Shared.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;

namespace Academy.Core.Abstraction.Infrastructure
{
    public interface IAcademyDbContext
    {
        DbSet<Employee> Employees { get; set; }
        DbSet<RoleMaster> RoleMasters { get; set; }
        DbSet<SeniorityMaster> SeniorityMasters { get; set; }
        DbSet<EmployeeRoleMap> EmployeeRoleMaps { get; set; }
        DbSet<EmployeeTrainingMap> EmployeeTrainingMaps { get; set; }
        DbSet<SkillMaster> SkillMasters { get; set; }
        DbSet<TrainingMaster> TrainingMasters { get; set; }
        DbSet<TrainingProficiencyMap> TrainingProficiencyMaps { get; set; }
        DbSet<EcosystemMaster> EcosystemMasters { get; set; }
        DbSet<ProficiencyMaster> ProficiencyMasters { get; set; }
        DbSet<SkillEndorsementMap> SkillEndorsementMaps { get; set; }
        DbSet<Comment> Comments { get; set; }
        DbSet<EmployeeTrainingReminder> Reminders { get; set; }
        DbSet<EmailDump> EmailDumps { get; set; }
        DbSet<Dashboard> Dashboards { get; set; }
        DbSet<CategoryMaster> CategoryMaster { get; set; }
        DbSet<JobRequest> JobRequests { get; set; }
        DbSet<JobRequestDetail> JobRequestDetails { get; set; }
        DbSet<Configuration> Configurations { get; set; }
        DbSet<ActivityMaster> ActivityMasters { get; set; }
        DbSet<DojoDetail> DojoDetails { get; set; }
        DbSet<EmployeeActivityMap> EmployeeActivityMaps { get; set; }
        DbSet<DojoGxLeaderAssignment> DojoGxLeaderAssignments { get; set; }
        DbSet<ProposedDojoGxLeader> ProposedDojoGxLeaders { get; set; }

        DbSet<ScheduledJob> ScheduledJobs { get; set; }

        DbSet<ReportColumnConfiguration> ReportColumnConfigurations { get; set; }
        DbSet<ReportType> ReportTypes { get; set; }
        DbSet<BookMarkTemplates> BookMarkTemplates { get; set; }
        DbSet<LearningPath> LearningPaths { get; set; }
        DbSet<LearningPathTrainingMap> LearningPathTrainingMaps { get; set; }
        DbSet<DojoProjectConfiguration> DojoProjectConfigurations { get; set; }
        DbSet<EmployeeDocumentTypeMaster> EmployeeDocumentTypes { get; set; }
        DbSet<Position> Positions { get; set; }
        DbSet<PositionSkill> PositionSkills { get; set; }

        Task<string> ExecuteStoredProcedureAsync(string procedureName, params SqlParameter[] parameters);
        Task<DataTable> ExecuteStoredProcedureDataTableAsync(string procedureName, params SqlParameter[] parameters);
        DbSet<usp_FetchEmployeeTrainings> usp_FetchEmployeeTrainings { get; set; }
        DbSet<TrainingStatusMaster> TrainingStatusMasters { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        #region AGK Migration
        DbSet<Community> Community { get; set; }
        DbSet<CommunityGKFocal> CommunityGKFocal { get; set; }
        DbSet<CommunitySelectionRatio> CommunitySelectionRatio { get; set; }
        DbSet<Defaulters> Defaulters { get; set; }
        DbSet<InterviewPanelDetails> InterviewPanelDetails { get; set; }
        DbSet<AllPanelSlots> AllPanelSlots { get; set; }
        DbSet<PanelSlots> PanelSlots { get; set; }
        DbSet<PanelSlotsRequirement> PanelSlotsRequirement { get; set; }
        DbSet<PanelUserCredential> PanelUserCredential { get; set; }
        DbSet<Domain.Entities.InterviewData> InterviewData { get; set; }
        DbSet<PanelType> PanelType { get; set; }
        DbSet<EmployeeCommunityMap> EmployeeCommunityMap { get; set; }
        DbSet<PanelDetails> PanelDetails { get; set; }


        
        #endregion
    }
}
