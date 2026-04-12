using Academy.Core.Abstraction.Infrastructure;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Academy.Core.Models;
using Academy.Shared.Constants;

namespace Academy.Workers.TrainingAssignmentWorker
{
    public class TrainingAssignmentService : BackgroundService
    {
        //Read pending entry from jobRequest table for request type = 'Training Assignment'
        //Process Metadata and insert entries in JobRequestDetail table
        //If ForceChecked then assign training without checking anything
        //Else
        //  Get the employee seniority and current proficiency
        //  Get expected skill proficiency
        //  Check history if same training was already assigned or not.
        //  Based on above information either assign training and update status in jobRequestDetail according
        //  OR if training is not to be assigned either due to proficiency match or past record, update status and comment in jobRequestDetail accordingly.

        private readonly AppSetting _appSetting;
        private readonly IServiceProvider _serviceProvider;
        private IAcademyDbContext _academyDbContext;
        private IGoogleApiManager _googleApiManager;
        private IAdoClient<AcademyDbSetting> _academyAdoClient;
        public TrainingAssignmentService(IOptions<AppSetting> appSetting, IServiceProvider serviceProvider)
        {
            _appSetting = appSetting.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_appSetting.TrainingAssignmentWorkerConfig.Enabled)
            {
                Console.WriteLine("Training Assignment worker is not enabled");
                return;
            }
            while (true)
            {
                await Do_Work();
            }
        }

        private async Task Do_Work()
        {
            Console.WriteLine("[TrainingAssignmentService >> Do_Work] Started...");

            using var scope = _serviceProvider.CreateScope();
            _academyDbContext = scope.ServiceProvider.GetService<IAcademyDbContext>();
            _googleApiManager = scope.ServiceProvider.GetService<IGoogleApiManager>();
            _academyAdoClient = scope.ServiceProvider.GetService<IAdoClient<AcademyDbSetting>>();
            JobRequest jobRequest = await FetchNextJobToProcess(JobRequestType.TrainingAssignment.ToString());
            if (jobRequest == null)
            {
                Console.WriteLine("No requests to process.");
            }
            else
            {
                try
                {
                    var metadata = jobRequest.RequestMetadata;
                    string content = await _googleApiManager.ReadFileContent(metadata);
                    Console.WriteLine("[TrainingAssignmentService >> Do_Work] Reading File From GDrive Completed...");
                    SpinTrainingRequest request = JsonConvert.DeserializeObject<SpinTrainingRequest>(content);
                    await ExtractJobDetails(jobRequest, request);
                    Dictionary<string, object> iParam = new()
                    {
                        { DbConstants.PARAM_FORCE, request.Force },
                        { DbConstants.PARAM_TRANSACTION_ID, jobRequest.TransactionId },
                        { DbConstants.PARAM_TRAINING_ASSIGNMENT_SRC, request.TrainingAssignmentSrc }
                    };
                    int result = await _academyAdoClient.ExecuteNonQueryAsync(DbConstants.EXECUTE_TRAINING_ASSIGNMENT, iParam);
                    Console.WriteLine($"[TrainingAssignmentService >> Do_Work] Assignment Execution Completed...Total Rows Affected: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TrainingAssignmentService >> Do_Work] Exception Occured...{ex.Message}");
                }
            }
            Console.WriteLine("[TrainingAssignmentService >> Do_Work] Completed...");
        }

        private async Task ExtractJobDetails(JobRequest job, SpinTrainingRequest request)
        {
            Console.WriteLine("[TrainingAssignmentService >> ExtractJobDetails] Started...");
            try
            {
                List<JobRequestDetail> details = [];
                foreach (UserTrainingMapping item in request.Mapping)
                {
                    foreach (EcosystemTraining training in item.Trainings)
                    {
                        JobRequestDetail record = new()
                        {
                            CreatedBy = job.CreatedBy,
                            CreatedOn = DateTime.UtcNow,
                            Comment = string.Empty,
                            GlobantEmailAddress = item.UserEmail,
                            IsActive = true,
                            Key = JsonConvert.SerializeObject(new { training.SkillId, training.TrainingId }),
                            Value = training.TrainingName,
                            Status = TrainingStatus.Pending.ToString(),
                            TransactionId = job.TransactionId
                        };
                        details.Add(record);
                    }
                }
                await _academyDbContext.JobRequestDetails.AddRangeAsync(details);
                job.Status = TrainingStatus.Ongoing.ToString();
                _academyDbContext.JobRequests.Update(job);
                int result = await _academyDbContext.SaveChangesAsync();
                Console.WriteLine($"[TrainingAssignmentService >> ExtractJobDetails] Job Extract Process Completed...Total Rows Affected: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrainingAssignmentService >> ExtractJobDetails] Exception Occured...{ex.Message}");
            }
            Console.WriteLine("[TrainingAssignmentService >> ExtractJobDetails] Completed...");
        }
        private async Task<JobRequest> FetchNextJobToProcess(string jobType)
        {
            return await _academyDbContext.JobRequests.FirstOrDefaultAsync(x => x.Status == TrainingStatus.Pending.ToString() && x.RequestType == JobRequestType.TrainingAssignment.ToString());
        }


        #region COMMENTED_UNUSED_CODE_BUT_DO_NOT_DELETE
        // BELOW METHOD IS NOT IN USE, BUT DO NOT DELETE
        private async Task ForceAssignTrainings(JobRequest jobrequest, SpinTrainingRequest request)
        {
            Console.WriteLine("ForceAssignTrainings started...");
            var training = await _academyDbContext.JobRequestDetails.FirstOrDefaultAsync(x => x.TransactionId == jobrequest.TransactionId && x.Status == TrainingStatus.Pending.ToString());
            Employee emp = await _academyDbContext.Employees.FirstOrDefaultAsync(x => x.GlobantEmailAddress == training.GlobantEmailAddress && x.IsActive);
            int result = 0;
            try
            {
                Dictionary<string, object> skill_and_training = JsonConvert.DeserializeObject<Dictionary<string, object>>(training.Key);
                EmployeeTrainingMap _request = new()
                {
                    ActualEndDate = null,
                    StartDate = DateTime.UtcNow,
                    ExpectedEndDate = DateTime.UtcNow.AddDays(2),
                    CreatedBy = jobrequest.CreatedBy,
                    CreatedOn = DateTime.UtcNow,
                    EmployeeId = emp.Id,
                    IsActive = true,
                    SkillId = short.Parse(skill_and_training["SkillId"].ToString()),
                    TrainingId = int.Parse(skill_and_training["TrainingId"].ToString()),
                    TrainingStatusId = (int)TrainingStatus.Pending,
                    TrainingTimeAccount = emp.Client,
                    TrainingTimeSeniorityId = emp.SeniorityId
                };
                await _academyDbContext.EmployeeTrainingMaps.AddAsync(_request);

                //TODO: add to email dump
                _academyDbContext.JobRequestDetails.Update(training);
                result = await _academyDbContext.SaveChangesAsync();
                if (result == 0)
                {
                    throw new Exception("Data Not Saved");
                }
                training.UpdatedOn = DateTime.UtcNow;
                training.UpdatedBy = _appSetting.SystemUser;
                training.Status = TrainingStatus.Completed.ToString();
            }
            catch (Exception ex)
            {
                training.UpdatedOn = DateTime.UtcNow;
                training.UpdatedBy = _appSetting.SystemUser;
                training.Status = TrainingStatus.Completed.ToString();
                training.Comment = ex.Message;
            }
            finally
            {
                _academyDbContext.JobRequestDetails.Update(training);
                await _academyDbContext.SaveChangesAsync();
            }
        }
        // BELOW METHOD IS NOT IN USE, BUT DO NOT DELETE
        private async Task AssignTrainings(JobRequest jobrequest, SpinTrainingRequest request)
        {
            Console.WriteLine("AssignTrainings started...");
            var training = await _academyDbContext.JobRequestDetails.FirstOrDefaultAsync(x => x.TransactionId == jobrequest.TransactionId && x.Status == TrainingStatus.Pending.ToString());
            Employee employee = await _academyDbContext.Employees.FirstOrDefaultAsync(x => x.GlobantEmailAddress == training.GlobantEmailAddress && x.IsActive);
            // Fetch list of all trainings for the Ecosystem employee belongs to, and for Employee Seniority
            List<TrainingProficiencyMap> proficiencyTrainingMaps = await (from T in _academyDbContext.TrainingProficiencyMaps
                                                                          join E in _academyDbContext.EcosystemMasters
                                                                            on T.EcosystemId equals E.EcosystemId
                                                                          where T.SeniorityId == employee.SeniorityId
                                                                            && (E.EcosystemId == employee.EcosystemId || E.ParentEcosystemId == employee.EcosystemId)
                                                                            && E.IsActive == true
                                                                          select T).ToListAsync();
            EmployeeTrainingMap employeeTraining = await (from ET in _academyDbContext.EmployeeTrainingMaps
                                                          where ET.EmployeeId == employee.Id
                                                          && ET.TrainingId.ToString() == training.Key.ToString()
                                                          select ET).FirstOrDefaultAsync();
            if (employeeTraining is not null)
            {
                training.Comment = "This training has already been assigned to the user";
            }
            else
            {
                EmployeeTrainingMap _request = new()
                {
                    ActualEndDate = null,
                    StartDate = DateTime.UtcNow,
                    ExpectedEndDate = DateTime.UtcNow.AddDays(2),
                    CreatedBy = jobrequest.CreatedBy,
                    CreatedOn = DateTime.UtcNow,
                    EmployeeId = employee.Id,
                    IsActive = true,
                    SkillId = 0,
                    TrainingId = int.Parse(training.Key),
                    TrainingStatusId = (int)TrainingStatus.Pending,
                    TrainingTimeAccount = employee.Client,
                    TrainingTimeSeniorityId = employee.SeniorityId
                };
            }
            training.Status = TrainingStatus.Completed.ToString();
            training.UpdatedBy = _appSetting.SystemUser;
            training.UpdatedOn = DateTime.UtcNow;
            int result = await _academyDbContext.SaveChangesAsync();
        }
        #endregion
    }
}
