using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Models;
using Academy.Infrastructure.AdoClient;
using Academy.Infrastructure.EF;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Arch.EntityFrameworkCore.UnitOfWork;
using Academy.Infrastructure.EmailClient;
using Academy.Infrastructure.GoogleClient;
using Google.Apis.Auth.OAuth2;
using Academy.Shared.Extensions;
using Academy.Infrastructure.CollaboratorClient;

namespace Academy.Infrastructure
{
    public static class DependencyExtension
    {
        public static void AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRestClient, RestClient>();
            string academyConnStr = configuration.GetConnectionString("Academy");
            IAdoSetting acDbSetting = new AcademyDbSetting(academyConnStr);
            services.AddTransient<IAdoClient<AcademyDbSetting>>(provider => new SqlServer<AcademyDbSetting>(acDbSetting));
            services.AddTransient<ISchemaInspector>(provider => new SqlServer<AcademyDbSetting>(acDbSetting));

            services.AddDbContext<AcademyDbContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseSqlServer(acDbSetting.ConnectionString, x => x.CommandTimeout(300)))
                       .AddUnitOfWork<AcademyDbContext>();

            services.AddScoped<IAcademyDbContext, AcademyDbContext>();

            ISmtpSettings globantSmtpSettings = new GlobantSmtpSetting(configuration);
            ISmtpSettings brevoSmtpSettings = new BrevoSmtpSetting(configuration);
            services.AddTransient<ISmtp<GlobantSmtpSetting>>(provider => new Smtp<GlobantSmtpSetting>(globantSmtpSettings));
            services.AddTransient<ISmtp<BrevoSmtpSetting>>(provider => new Smtp<BrevoSmtpSetting>(brevoSmtpSettings));

            services.AddScoped<IGoogleApiManager, GoogleApiManager>();

            services.AddSingleton<ICollaboratorClient, SlackCollaboratorClient>();
            
        }
    }
}