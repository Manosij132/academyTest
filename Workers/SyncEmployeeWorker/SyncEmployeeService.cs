using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Models;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Arch.EntityFrameworkCore.UnitOfWork;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Academy.Workers.SyncEmployeeWorker
{
    public class SyncEmployeeService : BackgroundService
    {
        private readonly AppSetting _appSetting;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _runTime;
        private IUnitOfWork? _unitOfWork;
        private IRepository<Employee>? _repEmployee;
        private IList<IList<object>>? sheetData;
        private EmployeeSheetColumnIndex? _headers;
        private ConcurrentBag<Employee> employees = new();
        private string errorMessage = "{0} is null or empty";
        private IAdoClient<AcademyDbSetting>? _adoClient;
        private IRepository<JobRequest>? _repJobRequest;
        private ConcurrentBag<Employee> employeeList = new();

        public SyncEmployeeService(IOptions<AppSetting> appSetting, IServiceProvider serviceProvider)
        {
            _appSetting = appSetting.Value;
            _serviceProvider = serviceProvider;
            _runTime = new TimeSpan(10, 0, 0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_appSetting.SyncEmployeeWorkerConfig.Enabled)
            {
                Console.WriteLine("Sync Employee worker is not enabled");
                return;
            }
            var now = DateTime.Now;
            var nextRunTime = now.Date.Add(_runTime);
            if (now > nextRunTime)
            {
                // Schedule for the next day if the time has already passed today
                nextRunTime = nextRunTime.AddDays(1);
            }
            var delay = nextRunTime - now;
            await Task.Delay(delay, stoppingToken);
            await Do_Work();
        }

        private async Task Do_Work()
        {
            Console.WriteLine("[SyncEmployeeService >> Do_Work] Started...");
            using (var scope = _serviceProvider.CreateScope())
            {
                _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                _repEmployee = _unitOfWork.GetRepository<Employee>();
                _adoClient = scope.ServiceProvider.GetRequiredService<IAdoClient<AcademyDbSetting>>();
                _repJobRequest = _unitOfWork.GetRepository<JobRequest>();
                try
                {
                    IGoogleApiManager _googleClient = scope.ServiceProvider.GetRequiredService<IGoogleApiManager>();
                    string range = $"{_appSetting.SheetDirectory.SyncDataSheetName}!{_appSetting.SheetDirectory.SyncDataRange}";
                    var sheetDict = await _googleClient.ReadRawDataFromWorksheetBySheetName(_appSetting.SheetDirectory.SyncDataWorksheetId, range);
                    sheetData = sheetDict[range];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if (sheetData is not null)
                {
                    _headers = new(sheetData[0]);

                    sheetData.RemoveAt(0);

                    Expression<Func<Employee, bool>> predicate = e => !string.IsNullOrWhiteSpace(e.GlobantEmailAddress) && e.Id != _appSetting.SystemUser;

                    int? count = _repEmployee?.Count(predicate: predicate);

                    IPagedList<Employee>? result = await _repEmployee?.GetPagedListAsync(predicate: predicate, pageIndex: 0, pageSize: count.Value);

                    if (result is not null)
                    {
                        employees = [.. result.Items];
                    }

                    await ProcessCreate();
                    await ProcessDelete();
                    await ProcessUpdate();
                    result = await _repEmployee?.GetPagedListAsync(predicate: predicate, pageIndex: 0, pageSize: count.Value);
                    if (result is not null)
                    {
                        employees = [.. result.Items];
                    }
                }
            }
            Console.WriteLine("[SyncEmployeeService >> Do_Work] Started...");
        }

        // Create new which are available in SheetData but Not in Employee DB
        private async Task ProcessCreate()
        {
            object lockObject = new();
            string trnx = DateTime.UtcNow.ToString(_appSetting.DateTimeAsIdFormat);
            Console.WriteLine("[SyncEmployeeService >> Do_Work >> ProcessCreate] started...");
            try
            {
                JobRequest job = new()
                {
                    CreatedBy = _appSetting.SystemUser,
                    CreatedOn = DateTime.UtcNow,
                    HasErrors = false,
                    IsActive = true,
                    RequestType = JobRequestType.AutoTrainingAssignment.ToString(),
                    RetryCount = 0,
                    Status = TrainingStatus.Pending.ToString(),
                    TransactionId = trnx,
                };
                List<JobRequestDetail> job_detail = [];
                if (sheetData is not null && _headers is not null)
                {
                    List<string> emailAddressesFromSheet = sheetData.Select(row => row[_headers.indexOfEmployeeEmail].ToString().ToLower().Trim()).ToList();
                    List<string> emailAddressesFromDB = employees.Select(e => e.GlobantEmailAddress.ToString().ToLower().Trim()).ToList();
                    List<string> newEntries = emailAddressesFromSheet.Except(emailAddressesFromDB).ToList();
                    var newRecords = sheetData.Where(x => newEntries.Contains(x[_headers.indexOfEmployeeEmail]?.ToString()?.ToLower()?.Trim())).ToList();
                    // Clear the lists to free memory
                    emailAddressesFromSheet = emailAddressesFromDB = newEntries = [];
                    ConcurrentBag<Employee> tobeAddedEmployees = [];
                    if (newRecords.Count > 0)
                    {
                        if (_unitOfWork is not null)
                        {
                            if (employees is not null && _repEmployee is not null)
                            {
                                await Parallel.ForEachAsync(newRecords, async (data, cancellationToken) =>
                                {
                                    lock (lockObject)
                                    {
                                        string? snr = data[_headers.indexOfSeniority].ToString()?.ToUpper().Trim();
                                        tobeAddedEmployees.Add(new Employee()
                                        {
                                            GlobantEmailAddress = data[_headers.indexOfEmployeeEmail].ToString()?.ToLower().Trim(),
                                            EmployeeName = data[_headers.indexOfEmployeeName].ToString()?.Trim(),
                                            BetterMeLeaderEmail = data[_headers.indexOfLeaderEmail].ToString()?.ToLower().Trim(),
                                            GexLeaders = data[_headers.indexOfGexLeaders].ToString()?.ToLower().Trim(),
                                            BaseLocation = data[_headers.indexOfBaseLocation].ToString()?.Trim(),
                                            Community = data[_headers.indexOfCommunity].ToString()?.Trim(),
                                            Position = data[_headers.indexOfPosition].ToString()?.Trim(),
                                            Designation = data[_headers.indexOfPosition].ToString()?.Trim(),
                                            Seniority = snr,
                                            CreatedBy = _appSetting.SystemUser,
                                            CreatedOn = DateTime.UtcNow,
                                            IsActive = true,
                                            Client = data[_headers.indexOfClient].ToString()?.Trim(),
                                            Project = data[_headers.indexOfProject].ToString()?.Trim(),
                                            GlobalId = data[_headers.indexOfGloberId].ToString(),
                                            Gender = data[_headers.indexOfGender].ToString(),
                                            JoiningDate = DateTime.TryParse(data[_headers.indexOfJoiningDate].ToString(), out DateTime jd) ? jd : DateTime.UtcNow,
                                            Status = data[_headers.indexOfStatus].ToString(),
                                            Tdc = data[_headers.indexOfTdc].ToString(),
                                            WorkingEcosystem = data[_headers.indexOfWorkingEcosystem].ToString(),
                                            TotalExperience = decimal.TryParse(data[_headers.indexOfTotalExperience].ToString(), out decimal te) ? te : 0,
                                            OtherInfo = trnx.ToString(),
                                        });
                                    }
                                });
                                await _repEmployee.InsertAsync(tobeAddedEmployees);
                                int result = await _unitOfWork.SaveChangesAsync();
                                // Once above Tasks completes, then run the below Stored Procedures to Create any Ecosystem,If not Exists
                                object responseE = await _adoClient.ExecuteScalerAsync(DbConstants.CREATE_ECOSYSTEM_IF_NOT_EXISTS, null);
                                Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessCreate >> {DbConstants.CREATE_ECOSYSTEM_IF_NOT_EXISTS}] executed...{responseE}");
                                object responseS = await _adoClient.ExecuteScalerAsync(DbConstants.MAP_SENIORITY, null);
                                Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessCreate >> {DbConstants.MAP_SENIORITY}] executed...{responseS}");
                                if (result > 0)
                                {
                                    Dictionary<string, object> iParams = new()
                                    {
                                        { "@transactionId", trnx },
                                        { "@bcc", _appSetting.DailyRemindersBcc }
                                    };
                                    int tResult = await _adoClient.ExecuteNonQueryAsync(DbConstants.EXECUTE_AUTO_TRAINING_ASSIGNMENT, iParams);
                                }
                                newRecords = [];
                                Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessCreate] Executed successfully...{result} rows affected");
                            }
                            else
                            {
                                throw new NullReferenceException("Employee and/or Repository are not defined");
                            }
                        }
                        else
                        {
                            throw new NullReferenceException("sheetData and/or headers are not defined");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessCreate] Exception {ex.Message}...");
            }
            finally
            {
                Console.WriteLine("[SyncEmployeeService >> Do_Work >> ProcessCreate] Completed...");
            }
        }

        // Update few properties, for which records are available in SheetData and Employee DB
        private async Task ProcessUpdate()
        {
            object lockObj = new object();
            Console.WriteLine("[SyncEmployeeService >> Do_Work >> ProcessUpdate] started....");
            try
            {
                if (sheetData is not null && _headers is not null)
                {
                    if (employees is not null && _repEmployee is not null)
                    {
                        List<string> emailAddressesFromSheet = sheetData.Select(row => row[_headers.indexOfEmployeeEmail].ToString().ToLower().Trim()).ToList();
                        List<string> emailAddressesFromDB = employees.Where(m => m.IsActive).Select(e => e.GlobantEmailAddress.ToString().ToLower().Trim()).ToList();
                        List<string> tobeUpdatedEntries = emailAddressesFromSheet.Intersect(emailAddressesFromDB).ToList();
                        ConcurrentBag<Employee> recordsToBeUpdated = new(employees.Where(x => tobeUpdatedEntries.Contains(x.GlobantEmailAddress)));

                        // Clear the lists to free memory
                        emailAddressesFromSheet = emailAddressesFromDB = tobeUpdatedEntries = new();

                        if (recordsToBeUpdated.Count > 0)
                        {
                            if (_unitOfWork is not null)
                            {
                                await Parallel.ForEachAsync(recordsToBeUpdated, async (data, cancellationToken) =>
                                {
                                    await Console.Out.WriteLineAsync("Inside ProcessUpdate > Parallel.ForEachAsync");
                                    var new_data = sheetData.FirstOrDefault(x => x[_headers.indexOfEmployeeEmail].ToString().ToLower().Trim() == data.GlobantEmailAddress.ToString().ToLower().Trim());
                                    string snr = new_data[_headers.indexOfSeniority].ToString()?.ToUpper().Trim();
                                    lock (lockObj)
                                    {
                                        data.IsActive = true;
                                        data.UpdatedBy = _appSetting.SystemUser;
                                        data.UpdatedOn = DateTime.UtcNow;
                                        data.BaseLocation = new_data[_headers.indexOfBaseLocation].ToString();
                                        data.Position = new_data[_headers.indexOfPosition].ToString();
                                        data.Seniority = snr;
                                        data.Client = new_data[_headers.indexOfClient].ToString();
                                        data.BetterMeLeaderEmail = new_data[_headers.indexOfLeaderEmail].ToString();
                                        data.GexLeaders = new_data[_headers.indexOfGexLeaders].ToString();
                                        data.Community = new_data[_headers.indexOfCommunity].ToString();
                                        data.Tdc = new_data[_headers.indexOfTdc].ToString();
                                        data.OtherInfo = new_data[_headers.indexOfWorkingEcosystem].ToString();
                                        data.Designation = new_data[_headers.indexOfPosition].ToString();
                                    }
                                });
                                _repEmployee.Update(recordsToBeUpdated);
                                int result = await _unitOfWork.SaveChangesAsync();
                                recordsToBeUpdated = new();
                                Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessUpdaed] executed successfully...{result} rows affected");
                            }
                            else
                            {
                                throw new NullReferenceException("UnitOfWork is not defined");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessUpdaed]... NO records found for Archieval");
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("Employee and/or Repository are not defined");
                    }
                }
                else
                {
                    throw new NullReferenceException("sheetData and/or headers are not defined");
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }
            finally
            {
                Console.WriteLine("[SyncEmployeeService >> Do_Work >> ProcessUpdate] completed....");
            }
        }

        // Mark IsActive = false which are available in Employee DB but Not in SheetData 
        private async Task ProcessDelete()
        {
            Console.WriteLine("[SyncEmployeeService >> Do_Work >> ProcessDelete] started....");
            object lockObj = new();
            try
            {
                if (sheetData is not null && _headers is not null)
                {
                    if (employees is not null && _repEmployee is not null)
                    {
                        List<string> emailAddressesFromSheet = sheetData.Select(row => row[_headers.indexOfEmployeeEmail].ToString().ToLower().Trim()).ToList();
                        List<string> emailAddressesFromDB = employees.Select(e => e.GlobantEmailAddress.ToString().ToLower().Trim()).ToList();
                        List<string> tobeDeletedEntries = emailAddressesFromDB.Except(emailAddressesFromSheet).ToList();
                        ConcurrentBag<Employee> archievedRecords = new(employees.Where(x => tobeDeletedEntries.Contains(x.GlobantEmailAddress.ToLower().Trim())));
                        // Clear the lists to free memory
                        emailAddressesFromSheet = emailAddressesFromDB = tobeDeletedEntries = new();
                        if (archievedRecords.Count > 0)
                        {
                            if (_unitOfWork is not null)
                            {
                                await Parallel.ForEachAsync(archievedRecords, async (data, cancellationToken) =>
                                {
                                    await Console.Out.WriteLineAsync("Inside ProcessDelete > Parallel.ForEachAsync");
                                    lock (lockObj)
                                    {
                                        data.IsActive = false;
                                        data.UpdatedBy = _appSetting.SystemUser;
                                        data.UpdatedOn = DateTime.UtcNow;
                                    }
                                });
                                _repEmployee.Update(archievedRecords);
                                int result = await _unitOfWork.SaveChangesAsync();
                                archievedRecords = new();
                                Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessDelete] executed successfully...{result} rows affected");
                            }
                            else
                            {
                                throw new NullReferenceException("UnitOfWork is not defined");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessDelete]... NO records found for Archieval");
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("Employee and/or Repository are not defined");
                    }
                }
                else
                {
                    throw new NullReferenceException("sheetData and/or headers are not defined");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SyncEmployeeService >> Do_Work >> ProcessDelete] Exception Occured.... {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[SyncEmployeeService >> Do_Work >> ProcessDelete] completed....");
            }
        }
    }
}