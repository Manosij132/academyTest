using Academy.API.Agents;
using Academy.API.Helpers;
using Academy.API.Models;
using Academy.Core;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Infrastructure;
using Academy.Shared.DTO;
using Academy.Workers.ReportWorker;

namespace Academy.API.Extensions
{
    public static class DependencyExtensions
    {
        public static void AddServiceDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerConfiguration();
            services.AddHttpContextAccessor();
            services.AddPredicates();
            services.AddCoreServices();
            services.AddInfrastructureDependencies(configuration);
            services.AddScoped<AppSettingsResolver>();
            // Register AgentNetwork as a singleton
            services.Configure<AgentOptions>(configuration);
            services.AddSingleton<AgentNetwork>();

            

            //#region Register Background Jobs
            //services.AddHostedService<ReportService>();
            //#endregion

            // Register RouterAgent as a singleton
            services.AddSingleton(sp =>
            {
                using (var scope = sp.CreateScope())
                {
                    var network = sp.GetRequiredService<AgentNetwork>();
                    var academyDBContext = scope.ServiceProvider.GetRequiredService<IAcademyDbContext>();

                    var openAISettings = configuration
                             .GetSection("AppSetting:OpenAISettings")
                             .Get<OpenAISettings>();

                    return RouterAgent.Create(
                    network,
                    academyDBContext,
                    openAISettings.ModelId,
                    openAISettings.ApiKey,
                    configuration["Environment"]
                );
                }
            });
        }
    }
}
