using Academy.API.Extensions;
using Academy.API.Helpers;
using Academy.API.Middlewares;
using Academy.Shared.DTO;
using Mapster;
using Microsoft.Extensions.Options;
using Staffing.Shared.Logging;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Prometheus;

namespace Academy.API
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            const string AcademyCorsPolicy = "AcademyCors";

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<AppSetting>(builder.Configuration.GetSection(nameof(AppSetting)));

            builder.Services.AddServiceDependencies(builder.Configuration);
            builder.Services.AddApiAuthentication(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: AcademyCorsPolicy,
                    cors =>
                    {
                        cors.AllowAnyHeader();
                        cors.AllowAnyMethod();
                        cors.WithOrigins(builder.Configuration["AllowedOrigins"].Split(",", StringSplitOptions.RemoveEmptyEntries));
                        cors.WithExposedHeaders("Content-Disposition");
                    });
            });
            
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            builder.Services.Configure<FileLoggerOptions>(builder.Configuration.GetSection("Logging").GetSection("FileLog"));
            builder.Services.AddMapster();

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var settings = httpContext.RequestServices
                        .GetRequiredService<IOptions<AppSetting>>().Value;

                    var userId = httpContext.User?.Identity?.Name;

                    if (string.IsNullOrEmpty(userId))
                    {
                        userId = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    }

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userId,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.NoOfRequestPerUser,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            var app = builder.Build();

            // Capture HTTP request metrics (labels for status code, method, etc.)
            app.UseHttpMetrics(); 

            using (var scope = app.Services.CreateScope())
            {
                var appSettings = scope.ServiceProvider.GetRequiredService<IOptions<AppSetting>>().Value;
                var settingsResolver = scope.ServiceProvider.GetRequiredService<AppSettingsResolver>();
                await settingsResolver.ResolveAsync(appSettings);
            }
            
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseMiddleware<GlobalErrorHandler>();

            app.UseCors(AcademyCorsPolicy);

            app.UseRateLimiter();

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/health") &&
                                    !context.Request.Path.StartsWithSegments("/api/Account") &&
                                    !context.Request.Path.StartsWithSegments("/api/Master/ad"),
                        context => context.UseMiddleware<CustomJwtMiddleware>());

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Map the metrics endpoint
            app.MapMetrics();

            app.Run();
        }
    }
}
