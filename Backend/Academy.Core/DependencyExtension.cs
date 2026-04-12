using Academy.ApplicationCore.Services;
using Academy.Core.Abstraction.Factories;
using Academy.Core.Abstraction.Services;
using Academy.Core.Factories;
using Academy.Core.PredicateBuilder;
using Academy.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Academy.Core
{
    public static class DependencyExtension
    {
        public static void AddPredicates(this IServiceCollection services)
        {
            services.AddTransient<SystemAdminPredicate>();
            services.AddTransient<CommunityAdminPredicate>();
            services.AddTransient<EcosystemAdminPredicate>();
            services.AddTransient<TdcAdminPredicate>();
            services.AddTransient<AccountAdminPredicate>();
            services.AddTransient<UserPredicate>();
            services.AddScoped<IPredicateFactory, PredicateFactory>();
        }

        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ISkillAndTrainingService, SkillAndTrainingService>();
            services.AddScoped<ISeniorityService, SeniorityService>();
            services.AddScoped<IEcosystemService, EcosystemService>();
            services.AddScoped<IProficiencyService, ProficiencyService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IDojoService, DojoService>();
            services.AddScoped<IReportService, ReportingService>();
            services.AddScoped<IBookMarkService, BookMarkService>();
            services.AddScoped<ISendEmailService, SendEmailService>();
            services.AddScoped<ITrainingService, TrainingService>();
            services.AddScoped<IReportDataService, ReportDataService>();
            services.AddScoped<ILangChainService, LangChainService>();
            services.AddScoped<IChatBotService, ChatBotService>();
            services.AddScoped<IAIService, AIService>();
            services.AddScoped<IGXLeaderService, GXLeaderService>();
            services.AddScoped<IInterviewPanelService, InterviewPanelService>();
            services.AddScoped<ICandidateProfileService, CandidateProfileService>();
            services.AddScoped<ISlotRequirementService, SlotRequirementService>();

            services.AddScoped<IJobsService, JobsService>();
        }
    }
}