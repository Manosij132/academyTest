using Microsoft.Extensions.Options;
using Prometheus;
using Staffing.API.Extensions;
using Staffing.API.Helpers;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using Staffing.Core.Abstraction.Repository;
using Staffing.Core.Abstraction.Services;
using Staffing.Shared.DTO;
using Staffing.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

const string StaffingCorsPolicy = "StaffingCors";
// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.Configure<AIConnection>(builder.Configuration.GetSection("StructuredSearch"));
builder.Services.AddSingleton<IAISettingsProvider, AISettingsProvider>();
builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
builder.Services.Configure<FileLoggerOptions>(builder.Configuration.GetSection("Logging").GetSection("FileLog"));
builder.Services.AddScoped<SqlServerDatabaseService>();
builder.Services.AddScoped<AIService>();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IChatClientService, ChatClientService>();
builder.Services.AddScoped<ISemanticKernelService, SemanticKernelService>();
builder.Services.AddScoped<IStaffRequestService, StaffRequestService>();
builder.Services.AddScoped<IStaffingSummaryService, StaffingSummaryService>();
builder.Services.AddScoped<AppSettingsResolver>();
builder.Services.AddApiAuthentication(builder.Configuration);



builder.Services.AddScoped<IChatHistoryRepository, ChatHistoryRepository>();
//
//builder.Services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
//add Authorization services
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: StaffingCorsPolicy,
        cors =>
        {
            cors.AllowAnyHeader();
            cors.AllowAnyMethod();
            //cors.AllowAnyOrigin();
            cors.WithOrigins(builder.Configuration["AllowedOrigins"].Split(",", StringSplitOptions.RemoveEmptyEntries));
        });
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(StaffingCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

 
app.MapControllers();
// Map the metrics endpoint
app.MapMetrics();
app.Run();
