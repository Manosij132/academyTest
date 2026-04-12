using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Core.Models;
using Academy.Core.Shared;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Academy.Core.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IDashboardService _dashboardService;
        private readonly ISkillAndTrainingService _skillAndTrainingService;
        private readonly ISchemaInspector _schemaInspector;
        private readonly IAIService _aIService;
        private readonly IAdoClient<AcademyDbSetting> _academyDB;
        public ChatBotService(IEmployeeService employeeService, IAcademyDbContext academyDbContext,
                              ISkillAndTrainingService skillAndTrainingService, IDashboardService dashboardService,
                              ISchemaInspector schemaInspector, IAdoClient<AcademyDbSetting> academyDB, IAIService aIService)
        {
            _employeeService = employeeService;
            _academyDbContext = academyDbContext;
            _skillAndTrainingService = skillAndTrainingService;
            _dashboardService = dashboardService;
            _schemaInspector=schemaInspector;
            _aIService = aIService;
            _academyDB = academyDB;
        }
        public async Task<ChatboartServiceResponse> ExecuteChatBotTrainingAssignment(string userEmail, string trainingName)
        {
            try
            {
                var employee = await _employeeService.FetchByEmail(userEmail);
                if (employee == null)
                {
                    return new ChatboartServiceResponse
                    {
                        Message = "Employee: " + userEmail + " not found",
                        IsSuccess = false
                    };
                }
                var ecosystemTrainingsResult = await _skillAndTrainingService
                    .FetchSkillTrainingsMetaData((int)employee.EcosystemId);
                var ecosystemTrainings = ecosystemTrainingsResult.Value;
                //check if training available
                var IsTraining = ecosystemTrainings
                    .SelectMany(skill => skill.Trainings)
                    .FirstOrDefault(t => t.TrainingName.ToLower() == trainingName.ToLower());
                if (IsTraining == null)
                {
                    return new ChatboartServiceResponse
                    {
                        Message = "Training: " + trainingName + " not found",
                        IsSuccess = false
                    };
                }
                // Flatten trainings into a single list with metadata
                var allTrainings = ecosystemTrainings
                    .SelectMany(skill => skill.Trainings, (skill, training) => new EcosystemTraining
                    {
                        SkillId = skill.SkillId,
                        TrainingName = training.TrainingName,
                        TrainingId = training.TrainingId,
                        TrainingLink = training.TrainingLink,
                        SeniorityId = training.SeniorityId,
                        Seniority = training.Seniority,
                        TrainingDescription = training.TrainingDescription,
                        TrainingCompletionHours = training.TrainingCompletionHours,
                        IsMvP = training.IsMvP
                    })
                    .ToList();

                // Filter trainings matching employee's seniority
                var userTrainings = allTrainings
                    .Where(t => t.SeniorityId == employee.SeniorityId)
                    .ToList();

                // Determine "parent" condition (all MVP)
                bool allMvpChecked = userTrainings.All(t => t.IsMvP);

                // Prepare user-training mapping
                var userTrainingMapping = new UserTrainingMapping
                {
                    UserId = employee.Id,
                    UserEmail = employee.GlobantEmailAddress,
                    SeniorityId = (int)employee.SeniorityId,
                    Seniority = employee.Seniority,
                    UserImage = string.Empty,
                    Parent = allMvpChecked,
                    SelectedTraning = [trainingName], // <-- You can populate this if needed
                    Trainings = userTrainings,
                };

                // Build final request
                var request = new SpinTrainingRequest
                {
                    Force = false,
                    Ecosystem = (int)employee.EcosystemId,
                    Account = employee.Client,
                    TrainingAssignmentSrc = "Globant Studios",
                    Mapping = [userTrainingMapping],
                    SelectedTraning = []
                };

                // Execute assignment
                var trnxIdResult = await _dashboardService.ExecuteTrainingAssignmentJob(request);
                string trnxId = trnxIdResult.Value;
                var result = trnxId;
                return new ChatboartServiceResponse
                {
                    Message = "Training assigned successfully. trnxId: " + trnxId,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new ChatboartServiceResponse
                {
                    Message = "Error: " + ex.Message,
                    IsSuccess = false
                };
            }
        }

        public async Task<List<Academy.Shared.DTO.EmployeeTrainingsResponse>> GetEmployeeTrainings(string email)
        {
            List<Academy.Shared.DTO.EmployeeTrainingsResponse> employeeTrainingRecordList = new();
            try
            {
                var result = await _academyDbContext.usp_FetchEmployeeTrainings.FromSqlInterpolated($"[dbo].[usp_FetchEmployeeTrainings] {email}").ToArrayAsync();

                if (result != null)
                {
                    foreach (var record in result)
                    {
                        employeeTrainingRecordList.Add(new Academy.Shared.DTO.EmployeeTrainingsResponse()
                        {
                            TrainingName = record.TrainingName,
                            TrainingStatus = record.TrainingStatus,
                            EmployeeEmail = email,
                            SkillName = record.SkillName,
                        });
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return employeeTrainingRecordList;
        }

        public IEnumerable<Employee> GetEmployees(string name)
        {
            var employees = _employeeService.FetchByName(name);
            return employees;
        }


        public async Task<List<Dictionary<string,string>>> ExecuteDynamicQuery(string prompt)
        {
           // string aiModel = "openai/gpt-4o-mini";
            string aiModel = "llama3.1";
            var databaseType = "SQL";

            // Get data schema
            var dbSchema = await _schemaInspector.GenerateSchemaAsync();

            //Generate Sql Query
            var dynamicQuery = await _aIService.GetAISQLQuery(aiModel, AIServices.Ollama, prompt, dbSchema, databaseType);

            //Only allow select operation
            if (!dynamicQuery.query.StartsWith("SELECT"))
            {
                throw new CustomException("Query modifies data, which is not allowed.");
            }
            //Run query
            var result = await _academyDB.ExecuteQueryAsJsonListAsync(dynamicQuery.query);

            return result.Take(1000).ToList();

        }
    }
}
