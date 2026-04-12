using Academy.Core.Abstraction.Infrastructure;
using Academy.Domain.Entities;
using Academy.Domain.StoreProcedureEntities;
using Academy.Shared.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace Academy.Infrastructure.EF
{
    public class AcademyDbContext : DbContext, IAcademyDbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<RoleMaster> RoleMasters { get; set; }
        public DbSet<SeniorityMaster> SeniorityMasters { get; set; }
        public DbSet<EmployeeRoleMap> EmployeeRoleMaps { get; set; }
        public DbSet<EmployeeTrainingMap> EmployeeTrainingMaps { get; set; }
        public DbSet<SkillMaster> SkillMasters { get; set; }
        public DbSet<TrainingMaster> TrainingMasters { get; set; }
        public DbSet<TrainingProficiencyMap> TrainingProficiencyMaps { get; set; }
        public DbSet<EcosystemMaster> EcosystemMasters { get; set; }
        public DbSet<SkillEndorsementMap> SkillEndorsementMaps { get; set; }
        public DbSet<EmployeeTrainingReminder> Reminders { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ProficiencyMaster> ProficiencyMasters { get; set; }
        public DbSet<EmailDump> EmailDumps { get; set; }
        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<JobRequest> JobRequests { get; set; }
        public DbSet<CategoryMaster> CategoryMaster { get; set; }
        public DbSet<JobRequestDetail> JobRequestDetails { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<ReportColumnConfiguration> ReportColumnConfigurations { get; set; }
        public DbSet<ReportType> ReportTypes { get; set; }
        public DbSet<BookMarkTemplates> BookMarkTemplates { get; set; }
        public DbSet<ActivityMaster> ActivityMasters { get; set; }
        public DbSet<DojoDetail> DojoDetails { get; set; }
        public DbSet<EmployeeActivityMap> EmployeeActivityMaps { get; set; }
        public DbSet<LearningPath> LearningPaths { get; set; }
        public DbSet<LearningPathTrainingMap> LearningPathTrainingMaps { get; set; }
        public DbSet<DojoProjectConfiguration> DojoProjectConfigurations { get; set; }
        public virtual DbSet<usp_FetchEmployeeTrainings> usp_FetchEmployeeTrainings { get; set; }
        public DbSet<TrainingStatusMaster> TrainingStatusMasters { get; set; }
        public DbSet<DojoGxLeaderAssignment> DojoGxLeaderAssignments { get; set; }
        public DbSet<ProposedDojoGxLeader> ProposedDojoGxLeaders { get; set; }
        public DbSet<ScheduledJob> ScheduledJobs { get; set; }

        public DbSet<EmployeeDocumentTypeMaster> EmployeeDocumentTypes { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<PositionSkill> PositionSkills { get; set; }

        #region AGK Migration
        public DbSet<Community> Community { get; set; }
        public DbSet<CommunityGKFocal> CommunityGKFocal { get; set; }
        public DbSet<CommunitySelectionRatio> CommunitySelectionRatio { get; set; }
        public DbSet<Defaulters> Defaulters { get; set; }
        public DbSet<InterviewPanelDetails> InterviewPanelDetails { get; set; }
        public DbSet<AllPanelSlots> AllPanelSlots { get; set; }
        public DbSet<PanelSlots> PanelSlots { get; set; }
        public DbSet<PanelSlotsRequirement> PanelSlotsRequirement { get; set; }
        public DbSet<PanelUserCredential> PanelUserCredential { get; set; }
        public DbSet<Domain.Entities.InterviewData> InterviewData { get; set; }
        public DbSet<PanelType> PanelType { get; set; }
        public DbSet<EmployeeCommunityMap> EmployeeCommunityMap { get; set; }
        public DbSet<PanelDetails> PanelDetails { get; set; }
        
        #endregion

        public AcademyDbContext(DbContextOptions<AcademyDbContext> options) : base(options)
        {
        }

        public async Task<string> ExecuteStoredProcedureAsync(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                var connection = Database.GetDbConnection();

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().ConfigureAwait(false); // Safely open EF-managed connection

                await using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters?.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                var result = new List<Dictionary<string, object>>();

                await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var row = Enumerable.Range(0, reader.FieldCount)
                        .ToDictionary(reader.GetName, i => reader.IsDBNull(i) ? null : reader.GetValue(i));
                    result.Add(row);
                }

                return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (SqlException ex)
            {
                throw; // Rethrow the exception after logging
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception after logging
            }
        }

        public async Task<DataTable> ExecuteStoredProcedureDataTableAsync(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                DataTable dataTable = new DataTable();

                var connection = Database.GetDbConnection();

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().ConfigureAwait(false); // Safely open EF-managed connection

                await using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters?.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                var result = new List<Dictionary<string, object>>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    dataTable.Load(reader); // Load data into DataTable
                }

                return dataTable;
            }
            catch (SqlException ex)
            {
                throw; // Rethrow the exception after logging
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception after logging
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dashboard>()
            // 2. Configure the 'ProficiencyScore' property
            .Property(d => d.ProficiencyScore)
            // 3. Set the precision and scale. 
            // Example: 5 total digits, 2 digits after the decimal point (e.g., 999.99)
            .HasPrecision(18, 2);

            modelBuilder.Entity<Employee>()
           // 2. Configure the 'ProficiencyScore' property
           .Property(d => d.Aging)
           // 3. Set the precision and scale. 
           // Example: 5 total digits, 2 digits after the decimal point (e.g., 999.99)
           .HasPrecision(18, 2);

            modelBuilder.Entity<Employee>()
           // 2. Configure the 'ProficiencyScore' property
           .Property(d => d.TotalExperience)
           // 3. Set the precision and scale. 
           // Example: 5 total digits, 2 digits after the decimal point (e.g., 999.99)
           .HasPrecision(18, 2);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable(nameof(Employee));
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<RoleMaster>(entity =>
            {
                entity.ToTable(nameof(RoleMaster));
                entity.HasKey(e => e.RoleId);
            });

            modelBuilder.Entity<SeniorityMaster>(entity =>
            {
                entity.ToTable(nameof(SeniorityMaster));
                entity.HasKey(e => e.SeniorityId);
            });

            modelBuilder.Entity<EmployeeRoleMap>(entity =>
            {
                entity.ToTable(nameof(EmployeeRoleMap));
                entity.HasKey(e => e.EmployeeRoleId);
            });

            modelBuilder.Entity<EmployeeTrainingMap>(entity =>
            {
                entity.ToTable(nameof(EmployeeTrainingMap));
                entity.HasKey(e => e.EmployeeTrainingId);
            });

            modelBuilder.Entity<SkillMaster>(entity =>
            {
                entity.ToTable(nameof(SkillMaster));
                entity.HasKey(e => e.SkillId);
            });

            modelBuilder.Entity<TrainingMaster>(entity =>
            {
                entity.ToTable(nameof(TrainingMaster));
                entity.HasKey(e => e.TrainingId);
            });

            modelBuilder.Entity<TrainingProficiencyMap>(entity =>
            {
                entity.ToTable(nameof(TrainingProficiencyMap));
                entity.HasKey(e => e.TrainingProficiencyId);
            });

            modelBuilder.Entity<EcosystemMaster>(entity =>
            {
                entity.ToTable(nameof(EcosystemMaster));
                entity.HasKey(e => e.EcosystemId);
            });

            modelBuilder.Entity<DojoProjectConfiguration>(entity =>
            {
                entity.ToTable("DojoProjectsConfiguration");
                entity.HasKey(e => e.DojoProjectsConfigurationId);
            });

            modelBuilder.Entity<SkillEndorsementMap>(entity =>
            {
                entity.ToTable(nameof(SkillEndorsementMap));
                entity.HasKey(e => e.SkillEndorsementId);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable(nameof(Comment));
                entity.HasKey(e => e.CommentId);
            });

            modelBuilder.Entity<EmployeeTrainingReminder>(entity =>
            {
                entity.ToTable(nameof(EmployeeTrainingReminder));
                entity.HasKey(e => e.EmployeeTrainingReminderId);
            });

            modelBuilder.Entity<EmailDump>(entity =>
            {
                entity.ToTable(nameof(EmailDump));
                entity.HasKey(e => e.EmailDumpId);
            });

            modelBuilder.Entity<ProficiencyMaster>(entity =>
            {
                entity.ToTable(nameof(ProficiencyMaster));
                entity.HasKey(e => e.ProficiencyId);
            });

            modelBuilder.Entity<Dashboard>(entity =>
            {
                entity.ToTable("vwDashboard");
                entity.HasNoKey();
            });

            modelBuilder.Entity<CategoryMaster>(entity =>
            {
                entity.ToTable(nameof(CategoryMaster));
                entity.HasKey(e => e.CategoryId);
            });

            modelBuilder.Entity<JobRequest>(entity =>
            {
                entity.ToTable(nameof(JobRequest));
                entity.HasKey(e => e.RequestId);
            });

            modelBuilder.Entity<JobRequest>()
               .HasIndex(j => j.TransactionId)
               .HasDatabaseName("ix_JobRequest_TransactionId")
               .IncludeProperties(j => new { j.RequestType, j.Status, j.HasErrors, j.ErrorDetail });

            modelBuilder.Entity<JobRequestDetail>(entity =>
            {
                entity.ToTable(nameof(JobRequestDetail));
                entity.HasKey(e => e.JobRequestDetailId);
            });

            modelBuilder.Entity<Configuration>(entity =>
            {
                entity.ToTable(nameof(Configuration));
                entity.HasKey(c => c.ConfigurationId);
            });

            modelBuilder.Entity<ActivityMaster>(entity =>
            {
                entity.ToTable(nameof(ActivityMaster));
                entity.HasKey(a => a.ActivityId);
            });

            modelBuilder.Entity<EmployeeActivityMap>(entity =>
            {
                entity.ToTable(nameof(EmployeeActivityMap));
                entity.HasKey(e => e.EmployeeActivityId);
            });

            modelBuilder.Entity<DojoDetail>(entity =>
            {
                entity.ToTable(nameof(DojoDetail));
                entity.HasKey(a => a.DojoDetailId);
            });
            modelBuilder.Entity<ReportColumnConfiguration>(entity =>
            {
                entity.ToTable(nameof(ReportColumnConfiguration));
                entity.HasKey(e => e.ReportColumnConfigId);
            });

            modelBuilder.Entity<ReportType>(entity =>
            {
                entity.ToTable(nameof(ReportType));
                entity.HasKey(e => e.ReportId);
            });

            modelBuilder.Entity<BookMarkTemplates>(entity =>
            {
                entity.ToTable(nameof(BookMarkTemplates));
                entity.HasKey(c => c.BookMarkId);
            });

            modelBuilder.Entity<LearningPath>(entity =>
            {
                entity.ToTable(nameof(LearningPath));
                entity.HasKey(e => e.LearningPathId);
            });

            modelBuilder.Entity<LearningPathTrainingMap>(entity =>
            {
                entity.ToTable(nameof(LearningPathTrainingMap));
                entity.HasKey(e => e.LearningPathId);
            });

            modelBuilder.Entity<usp_FetchEmployeeTrainings>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<TrainingStatusMaster>(entity =>
            {
                entity.ToTable(nameof(TrainingStatusMaster));
                entity.HasKey(t => t.TrainingStatusId);
            });

            modelBuilder.Entity<DojoGxLeaderAssignment>(entity =>
            {
                entity.ToTable(nameof(DojoGxLeaderAssignment));
                entity.HasKey(a => a.DojoGxLeaderAssignmentId);
            });

            modelBuilder.Entity<ProposedDojoGxLeader>(entity =>
            {
                entity.ToTable(nameof(ProposedDojoGxLeader));
                entity.HasKey(a => a.ProposedDojoGxLeaderId);
            });

            modelBuilder.Entity<ScheduledJob>(entity =>
            {
                entity.ToTable(nameof(ScheduledJob));
                entity.HasKey(a => a.ScheduledJobId);
            });

            modelBuilder.Entity<EmployeeDocumentTypeMaster>(entity =>
            {
                entity.ToTable(nameof(EmployeeDocumentTypeMaster));
                entity.HasKey(e => e.EmployeeDocumentTypeId);
            });

            modelBuilder.Entity<PanelModel>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.ToTable(nameof(Position));
                entity.HasKey(a => a.Id);
            });

            modelBuilder.Entity<PositionSkill>(entity =>
            {
                entity.ToTable(nameof(PositionSkill));
                entity.HasKey(a => a.Id);
            });
        }
    }
}
