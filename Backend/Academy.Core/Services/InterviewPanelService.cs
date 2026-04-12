using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Extensions;
using Arch.EntityFrameworkCore.Internal;
using Arch.EntityFrameworkCore.UnitOfWork;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using TimeZoneConverter;
using static Google.Apis.Calendar.v3.Data.Event;
using Event = Google.Apis.Calendar.v3.Data.Event;

namespace Academy.Core.Services
{
    //AGK API Migration

    public class InterviewPanelService: IInterviewPanelService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IRepository<InterviewPanelDetails> _InterviewPanelRepository;
        private readonly IRepository<Employee> _employee;
        private readonly IRepository<Community> _community;
        private readonly IRepository<SeniorityMaster> _seniority;  
        private readonly IRepository<PanelSlots> _panelSlots;
        private readonly IRepository<Defaulters> _defaulters;
        private readonly IRepository<PanelSlotsRequirement> _panelSlotRequirement;
        private readonly IRepository<CommunityGKFocal> _communityGKFocal;
        private readonly IRepository<PanelDetails> _panelDetails;
        private readonly IRepository<PanelUserCredential> _panelUserCredential;
        private readonly IRepository<EmployeeCommunityMap> _employeeCommunityMap;
        private readonly IRepository<Domain.Entities.PanelType> _panelType;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticatedUserService _authenticatedUserService;      
        private DateTime localDateTime;
        private readonly AppSetting _appSetting;

        public InterviewPanelService(IUnitOfWork unitOfWork,  IConfiguration configuration, IAuthenticatedUserService authenticatedUserService, IOptions<AppSetting> appSetting)
        {
            _InterviewPanelRepository = unitOfWork.GetRepository<InterviewPanelDetails>();
            _employee = unitOfWork.GetRepository<Employee>();
            _community = unitOfWork.GetRepository<Community>();
            _seniority = unitOfWork.GetRepository<SeniorityMaster>();
            _panelSlots = unitOfWork.GetRepository<PanelSlots>();
            _defaulters = unitOfWork.GetRepository<Defaulters>();
            _panelType = unitOfWork.GetRepository<PanelType>();
            _panelDetails = unitOfWork.GetRepository<PanelDetails>();
            _panelSlotRequirement = unitOfWork.GetRepository<PanelSlotsRequirement>();
            _authenticatedUserService = authenticatedUserService;
            _unitOfWork = unitOfWork;
            _communityGKFocal = unitOfWork.GetRepository<CommunityGKFocal>();
            _panelUserCredential = unitOfWork.GetRepository<PanelUserCredential>();
            _configuration = configuration;
            _appSetting = appSetting.Value;
        }
        public async Task<(List<InterviewPanelModel>, int, int)> GetAllInterviewPanelsData(InterviewPanelFilterModelRequest panelFilter, int pageNumber, int pageSize)
        {
            DateTime startDate = DateTime.Parse(panelFilter.StartDate).Date;
            DateTime endDate = DateTime.Parse(panelFilter.EndDate).Date.AddHours(23);
            int requiredSlots = GetRequiredSlots(startDate, endDate);

            //DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local);

            List<InterviewPanelModel> interviewPanelModels = new();
            IEnumerable<InterviewPanelDetails> panels = _InterviewPanelRepository.GetAll().ToList();

            if (panelFilter != null)
            {
                //Email Address and Name
                if (!string.IsNullOrEmpty(panelFilter.SearchTerm))
                {
                    panels = panels.Where(x => x.PrimaryPanel.GlobantEmailAddress.ToLower().Contains(panelFilter.SearchTerm.ToLower()) || x.PrimaryPanel.EmployeeName.ToLower().Contains(panelFilter.SearchTerm.ToLower()));
                }
                //Panel Types
                if (panelFilter.PanelTypes != null && panelFilter.PanelTypes.Count > 0)
                {
                    panels = panels.Where(x => panelFilter.PanelTypes.Contains(x.Type?.ToLower()));
                }
                //Seniority
                if (panelFilter.Seniorities != null && panelFilter.Seniorities.Count > 0)
                {
                    panels = panels.Where(x => panelFilter.Seniorities.Contains(x.SeniorityId));
                }
                //Community
                if (panelFilter.Communities != null && panelFilter.Communities.Count > 0)
                {
                    panels = panels.Where(x => panelFilter.Communities.Contains(x.CommunityId));
                }
                //TDCs
                if (panelFilter.TDCs != null && panelFilter.TDCs.Count > 0)
                {
                    panels = panels.Where(x => panelFilter.TDCs.Contains(x.TDC));
                }
            }

            int count = 0;
            var querydata = GetPanelModel(panels.ToList(), startDate, endDate, requiredSlots);

            if (panelFilter.AvailableSlots)
            {
                querydata = querydata.Where(x => x.SlotCount > 0).OrderByDescending(x => x.SlotCount).ToList();
            }
            else if (panelFilter.IsDeficit)
            {
                querydata = querydata.Where(x => x.Deficit > 0).OrderByDescending(x => x.Deficit).ToList();
            }
            else
            {
                querydata = querydata.Where(x => x.SlotCount > 0).OrderByDescending(x => x.SlotCount).ToList();
            }


            if (pageNumber > 0 && pageSize > 0)
            {
                interviewPanelModels = querydata.Skip(((pageNumber - 1) * pageSize)).Take(pageSize).ToList();
                count = querydata?.Count() ?? 0;
            }
            else
            {
                interviewPanelModels = querydata.ToList();
                count = querydata?.Count() ?? 0;
            }
            return new(interviewPanelModels, count, interviewPanelModels.Count);
        }

        private int GetRequiredSlots(DateTime startDate, DateTime endDate)
        {
            int requiredSlots = 0;
            var dateRange = SplitDateRange(startDate, endDate, 6);

            if (dateRange.Count() >= 1)
            {
                foreach (var item in dateRange)
                {
                    var dateDiff = (item.Item2 - item.Item1).Days;
                    if (dateDiff >= 6)
                    {
                        requiredSlots += InterviewContants.RequiredSlots;
                    }
                }
            }

            return requiredSlots;
        }


        private List<InterviewPanelModel> GetPanelModel(List<InterviewPanelDetails> panels, DateTime? startDate, DateTime? endDate, int requiredSlots = 0)
        {
            List<InterviewPanelModel> panelModel = new();
            try
            {
                var communityGKFocal = _communityGKFocal.GetPagedList().Items;

                var seniorities = _seniority.GetAll().ToList();
                var communities = _community.GetAll().ToList();
                var panelSlots = _panelSlots.GetAll().ToList();
                var defaulters = _defaulters.GetAll().ToList();
                var employees = _employee.GetAll().Select(x => new Employee { Id = x.Id, EmployeeName = x.EmployeeName, GlobantEmailAddress = x.GlobantEmailAddress, BetterMeLeaderEmail = x.BetterMeLeaderEmail });

                if (startDate.HasValue && endDate.HasValue)
                {
                    panelSlots = panelSlots.Where(p => p.SlotDate >= startDate && p.SlotDate <= endDate).ToList();
                    defaulters = defaulters.Where(d => d.StartDate >= startDate && d.EndDate <= endDate).ToList();
                }
                foreach (var panel in panels)
                {
                    var employee = employees.FirstOrDefault(x => x.Id == panel.PrimaryPanelId);
                    var communityDetails = communities.FirstOrDefault(x => x.Id == panel.CommunityId);

                    if (employee != null)
                    {
                        if (communityDetails == null)
                        {
                            communityDetails = GetEmployeeCommunity(employee.Id);
                        }

                        panelModel.Add(new InterviewPanelModel
                        {
                            Id = panel.Id,
                            EmailId = employee.GlobantEmailAddress,
                            GlobantLeaderEmailId = employee.BetterMeLeaderEmail,
                            PanelName = employee.EmployeeName,
                            CommunityId = communityDetails != null ? communityDetails.Id : 0,
                            CommunityName = communityDetails != null ? communityDetails.Name : string.Empty,
                            PanelType = panel.Type,
                            SeniorityId = panel.SeniorityId,
                            SeniorityName = seniorities.FirstOrDefault(x => x.SeniorityId == panel.SeniorityId).SeniorityName,
                            RequiredSlots = requiredSlots,
                            SlotCount = panelSlots.Where(p => p.PanelId == panel.Id).Count(),
                            NonUtilizedSlot = panelSlots.Where(p => p.PanelId == panel.Id && !p.IsUtilized).Count(),
                            Deficit = defaulters.Where(d => d.PanelId == panel.Id).Sum(x => x.DefaulterCount),
                            Quater = defaulters.Where(d => d.PanelId == panel.Id).Select(t => t.Quarter).FirstOrDefault(),
                            TDC = panel.TDC,
                            CommunityGKFocalEmailId = communityGKFocal.FirstOrDefault(g => g.CommunityId == panel.CommunityId) != null ? communityGKFocal.FirstOrDefault(g => g.CommunityId == panel.CommunityId).GKFocalEmailId : ""
                        });
                    }
                }
                return panelModel;
            }
            catch (Exception ex)
            {
                return panelModel;
            }

        }

        public Community GetEmployeeCommunity(int employeeId)
        {
            List<Community> communityDetails = new List<Community>();
            var employeeCommunityMap = _employeeCommunityMap.GetAll().Where(x => x.EmployeeId == employeeId).ToList();
            if (employeeCommunityMap != null)
            {
                communityDetails = _community.GetAll().Where(x => x.Id == employeeCommunityMap.FirstOrDefault().CommunityId).ToList();
            }
            return communityDetails != null && communityDetails.Count > 0 ? communityDetails.FirstOrDefault() : null;
        } 

        public async Task<DataTable> ExecuteStoredProcedure(string sqlQuery)
        {
           string _connectionString = _configuration.GetConnectionString("Academy").Decrypt();

            DataTable resultTable = new DataTable();
            // DataSet dataSet = new DataSet();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 3600;
                        // Execute the stored procedure and fill the DataTable
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(resultTable);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            DataTable stringDataTable = ConvertDataTableToString(resultTable);

            return stringDataTable;
        }

        static DataTable ConvertDataTableToString(DataTable originalTable)
        {
            // Create a new DataTable to hold string values
            DataTable stringTable = new DataTable();

            // Loop through each column in the original table
            foreach (DataColumn column in originalTable.Columns)
            {
                // Add a new column to the string table with the same name but of type string
                stringTable.Columns.Add(column.ColumnName, typeof(string));
            }

            // Loop through each row in the original table
            foreach (DataRow originalRow in originalTable.Rows)
            {
                // Create a new row for the string table
                DataRow stringRow = stringTable.NewRow();

                // Loop through each column and convert the value to a string
                for (int i = 0; i < originalTable.Columns.Count; i++)
                {
                    stringRow[i] = originalRow[i]?.ToString(); // Convert to string, handle nulls
                }

                // Add the new row to the string DataTable
                stringTable.Rows.Add(stringRow);
            }

            return stringTable;
        }

        private IEnumerable<Tuple<DateTime, DateTime>> SplitDateRange(DateTime start, DateTime end, int dayChunkSize)
        {
            DateTime chunkEnd;
            while ((chunkEnd = start.Date.AddDays(dayChunkSize).AddHours(23)) < end.Date.AddHours(23))
            {
                yield return Tuple.Create(start, chunkEnd);
                start = chunkEnd.AddDays(1);
            }
            yield return Tuple.Create(start, end);
        }

        private List<InterviewPanelModel> GetDashboardPanelModel(List<InterviewPanelDetails> InterviewPanelDetailsList, DateTime? startDate, DateTime? endDate)
        {
            List<InterviewPanelModel> panelModel = new();
            try
            {
                var communityGKFocal = _communityGKFocal.GetPagedList().Items;

                //var seniorities = _seniority.GetAll().ToList();
                var communities = _community.GetAll().ToList();
                var panelSlots = _panelSlots.GetAll().ToList();
                var defaulters = _defaulters.GetAll().ToList();
                //var employees = _employee.GetAll().Select(x => new Employee { Id = x.Id, EmployeeName = x.EmployeeName, GlobantEmailAddress = x.GlobantEmailAddress, BetterMeLeaderEmail = x.BetterMeLeaderEmail });

                if (startDate.HasValue && endDate.HasValue)
                {
                    panelSlots = panelSlots.Where(p => p.SlotDate >= startDate && p.SlotDate <= endDate).ToList();
                    defaulters = defaulters.Where(d => d.StartDate >= startDate && d.EndDate <= endDate).ToList();
                }
                foreach (var panel in InterviewPanelDetailsList)
                {
                    //var employe = employees.FirstOrDefault(x => x.Id == panel.PrimaryPanelId);

                    if (communities.FirstOrDefault(x => x.Id == panel.CommunityId) != null)
                    {
                        panelModel.Add(new InterviewPanelModel
                        {
                            Id = panel.Id,
                            CommunityId = communities.FirstOrDefault(x => x.Id == panel.CommunityId).Id,
                            CommunityName = communities.FirstOrDefault(x => x.Id == panel.CommunityId).Name,
                            PanelType = panel.Type,
                            SlotCount = panelSlots.Where(p => p.PanelId == panel.Id).Count(),
                            NonUtilizedSlot = panelSlots.Where(p => p.PanelId == panel.Id && !p.IsUtilized).Count(),
                            Deficit = defaulters.Where(d => d.PanelId == panel.Id).Sum(x => x.DefaulterCount),
                            PanelName = panel.PrimaryPanel?.EmployeeName,
                            SeniorityUpTo = panel.SeniorityUpTo,
                            EmailId = panel.PrimaryPanel?.GlobantEmailAddress,
                            Slots = panelSlots
                                    .Where(p => p.PanelId == panel.Id)
                                    .Select(p => new AllPanelSlots
                                    {
                                        Id = p.Id,
                                        SlotDate = p.SlotDate,
                                        IsUtilized = p.IsUtilized
                                    })
                                    .ToList()
                        });
                    }
                }
                return panelModel;
            }
            catch (Exception ex)
            {
                //_logger.LogError("Error occured in GetPanelModel, Error - " + ex.StackTrace);
                return panelModel;
            }

        }

        public async Task<List<PanelModel>> GetAllPanelData()
        {
            var query = _panelDetails.GetAll();
            return query.OrderByDescending(a => a.Name).Select(a => new PanelModel { Name = a.Name }).Distinct().ToList();
        }

        public async Task<List<TDCModel>> GetAllTDCData()
        {
            var query = _employee.GetAll().Where(t => t.Tdc != null);
            return query.OrderByDescending(a => a.Tdc)
                .Select(a => new TDCModel { TDCId = a.Tdc, TDCName = a.Tdc }).Distinct().ToList();
        }

        public async Task<List<CommunityModel>> GetAllCommunityData()
        {
            var query = _community.GetAll().Where(t => t.Name != null);
            return query.OrderByDescending(a => a.Name)
                .Select(a => new CommunityModel { CommunityId = a.Id, CommunityName = a.Name }).Distinct().ToList();
        }
        public async Task<List<SeniorityDto>> GetAllSeniorityData()
        {
            var query = _seniority.GetAll().Where(t => t.SeniorityName != null);
            return query.OrderByDescending(a => a.SeniorityName)
                .Select(a => new SeniorityDto { Id = a.SeniorityId, Name = a.SeniorityName }).Distinct().ToList();
        }

        public async Task<List<PanelSlotModel>> GetPanelSlotsDetail(int panelId)
        {
            var result = new List<PanelSlotModel>();
            result.AddRange(_panelSlots.GetAll().Where(x => x.PanelId == panelId).
                Select(a => new PanelSlotModel { Date = a.SlotDate.ToString("yyyy-MM-dd"), Title = a.SlotDate.ToString("hh:mm tt"), Color = a.IsUtilized ? InterviewContants.UtilizedColor : InterviewContants.UnutilizedColor, BackgroundColor = "" }).ToList());


            var defaulter = _defaulters.GetAll()?.ToList()?.Where(x => x.PanelId == panelId);

            foreach (var def in defaulter)
            {
                var startDate = def.StartDate?.Date;
                while (startDate <= def.EndDate?.Date)
                {
                    result.Add(new PanelSlotModel()
                    {
                        Date = startDate?.ToString("yyyy-MM-dd"),
                        Title = "",
                        BackgroundColor = InterviewContants.DefaulterColor
                    });
                    startDate = startDate?.AddDays(1);
                }
            }
            return result;
        }

        public async Task<bool> SendEmail(PanelSendEmailModel panelSendEmailModel)
        {
            try
            {
                using MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_configuration["InterviewEmail:FromAddress"], _configuration["InterviewEmail:FromName"]);
                mailMessage.Subject = panelSendEmailModel.Subject;
                mailMessage.Body = panelSendEmailModel.Body;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(panelSendEmailModel.GloberEmail);
                mailMessage.CC.Add(panelSendEmailModel.GloberLeaderEmail);

                using SmtpClient smtpClient = new SmtpClient(_configuration["InterviewEmail:SMTPAddress"]);
                smtpClient.Port = Convert.ToInt32(_configuration["InterviewEmail:SMTPPortNumber"]);
                smtpClient.Credentials = null;
                smtpClient.EnableSsl = false;
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<DashboardDataModel> GetDashboardData(DashboardFilterModel panelFilter)
        {
            DashboardDataModel dashboardDataModel = new DashboardDataModel();
            // _logger.LogInformation("Dashboard service started");
            DateTime startDate = DateTime.Parse(panelFilter.StartDate).Date;
            DateTime endDate = DateTime.Parse(panelFilter.EndDate).Date.AddHours(23);

            IEnumerable<InterviewPanelDetails> InterviewPanelDetailsList = _InterviewPanelRepository.GetAll().ToList();
            IEnumerable<PanelSlotsRequirement> requirment = _panelSlotRequirement.GetAll().ToList();
            if (panelFilter != null)
            {

                //Panel Types
                if (panelFilter.PanelTypes != null && panelFilter.PanelTypes.Count > 0)
                {
                    InterviewPanelDetailsList = InterviewPanelDetailsList.Where(x => panelFilter.PanelTypes.Contains(x.Type?.ToLower()));
                }
                //Seniority
                if (panelFilter.Seniorities != null && panelFilter.Seniorities.Count > 0)
                {
                    InterviewPanelDetailsList = InterviewPanelDetailsList.Where(x => panelFilter.Seniorities.Contains(x.SeniorityId));
                }
                //Community
                if (panelFilter.Communities != null && panelFilter.Communities.Count > 0)
                {
                    InterviewPanelDetailsList = InterviewPanelDetailsList.Where(x => panelFilter.Communities.Contains(x.CommunityId));
                    requirment = requirment.Where(x => panelFilter.Communities.Contains(x.CommunityId));
                }
                //TDCs
                if (panelFilter.TDCs != null && panelFilter.TDCs.Count > 0)
                {
                    InterviewPanelDetailsList = InterviewPanelDetailsList.Where(x => panelFilter.TDCs.Contains(x.TDC));
                    requirment = requirment.Where(x => panelFilter.TDCs.Contains(x.TDC));
                }
            }

            var interviewPanelModels = GetDashboardPanelModel(InterviewPanelDetailsList.ToList(), startDate, endDate);


            if (interviewPanelModels != null)
            {
                var requirmentList = requirment.Where(x => x.StartDate >= startDate && x.EndDate <= endDate).ToList();
                var L1Panels = interviewPanelModels.Where(x => x.PanelType == "L1");
                var gkPanels = interviewPanelModels.Where(x => x.PanelType == "GK");
                DashboardTilesDataModel dashboardTilesDataModel = new DashboardTilesDataModel()
                {
                    TotalSlots = interviewPanelModels.Sum(x => x.SlotCount),
                    L1Slots = L1Panels.Sum(x => x.SlotCount),
                    GKSlots = gkPanels.Sum(x => x.SlotCount),
                    L1UntilizedSlots = L1Panels.Sum(x => x.NonUtilizedSlot),
                    GKUnutilizedSlots = gkPanels.Sum(x => x.NonUtilizedSlot),
                    L1Deficit = requirmentList.Sum(x => x.L1SlotsRequired) - requirmentList.Sum(x => x.L1SlotsActual),
                    GKDeficit = requirmentList.Sum(x => x.GKSlotsRequired) - requirmentList.Sum(x => x.GKSlotsActual)
                };

                dashboardDataModel.DashboardTiles = dashboardTilesDataModel;

                //_logger.LogInformation("DashboardTilesDataModel completed");

                dashboardDataModel.CommunityChartDataModel = new();
                var communities = interviewPanelModels.Select(x => x.CommunityName).Distinct().OrderByDescending(x => x);
                foreach (var community in communities)
                {
                    dashboardDataModel.CommunityChartDataModel.Add(new ChartDataModel() { Name = community, Value = interviewPanelModels.Where(x => x.CommunityName == community).Select(x => x.SlotCount).Sum() });
                }

                dashboardDataModel.PanelTypeChartDataModel = new();
                var panelTypes = interviewPanelModels.Select(x => x.PanelType).Distinct().OrderByDescending(x => x);
                foreach (var panelType in panelTypes)
                {
                    dashboardDataModel.PanelTypeChartDataModel.Add(new ChartDataModel() { Name = panelType, Value = interviewPanelModels.Where(x => x.PanelType == panelType).Select(x => x.SlotCount).Sum() });
                }

                foreach (var panelData in interviewPanelModels)
                {
                    InterviewScheduleData interviewScheduleData = new InterviewScheduleData();

                    interviewScheduleData.Panel = panelData.PanelType;
                    interviewScheduleData.PanelId = panelData.Id;
                    interviewScheduleData.EmailId = panelData.EmailId;
                    interviewScheduleData.PrimaryPanel = panelData.PanelName;
                    interviewScheduleData.Slots = panelData.Slots;
                    interviewScheduleData.UpToSeniority = panelData.SeniorityUpTo;
                    interviewScheduleData.CommunityName = panelData.CommunityName;

                    if (dashboardDataModel.InterviewScheduleData == null)
                    {
                        dashboardDataModel.InterviewScheduleData = new List<InterviewScheduleData>();
                    }

                    dashboardDataModel.InterviewScheduleData.Add(interviewScheduleData);
                }
            }


            return dashboardDataModel;
        }


        public async Task<DashboardDataModel> GetInterviewPanelDetails(DashboardFilterModel panelFilter)
        {
            DashboardDataModel data = new DashboardDataModel();

            DateTime startDate = DateTime.Parse(panelFilter.StartDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime endDate = DateTime.Parse(panelFilter.EndDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
            endDate = endDate.AddDays(1);

            string formattedDateTimeStart = startDate.ToString("yyyy-MM-dd");
            string formattedDateTimeEnd = endDate.ToString("yyyy-MM-dd");

            string basequery = string.Empty;
            string whereQuery = string.Empty;
            string sortQuery = string.Empty;

            basequery = "select ipd.Id as PanelId,e.GlobantEmailAddress as EmailId,ipd.Type as Panel,e.EmployeeName as PrimaryPanel,  s.SeniorityName as UpToSeniority,ipd.SeniorityId, e.Community as CommunityName from [dbo].[InterviewPanelDetails] ipd inner join  [dbo].[Employee] e on ipd.PrimaryPanelid = e.Id inner join  [dbo].[SeniorityMaster] s on ipd.SeniorityId = s.SeniorityId  ";
            whereQuery = $" where 1=1 ";

            //TDCs
            if (panelFilter.TDCs != null && panelFilter.TDCs.Count > 0)
            {
                string TDCs = String.Join(", ", panelFilter.TDCs.Select(s => $"'{s}'"));
                whereQuery += $" and ipd.TDC in ({TDCs}) ";
            }

            //Community
            if (panelFilter.Communities != null && panelFilter.Communities.Count > 0)
            {
                string Communities = String.Join(", ", panelFilter.Communities.Select(s => $"'{s}'"));
                whereQuery += $" and ipd.CommunityId in ({Communities}) ";
            }

            //Seniority
            if (panelFilter.Seniorities != null && panelFilter.Seniorities.Count > 0)
            {
                int SenioritiesMax = panelFilter.Seniorities.Select(numStr => Convert.ToInt32(numStr)).Max();
                whereQuery += $" and ipd.SeniorityId >= {SenioritiesMax} ";
            }

            // PanelTypes
            if (panelFilter.PanelTypes != null && panelFilter.PanelTypes.Count > 0)
            {
                string PanelTypes = String.Join(", ", panelFilter.PanelTypes.Select(s => $"'{s}'"));
                whereQuery += $" and ipd.Type in ({PanelTypes}) ";
            }

            if (panelFilter.SearchTerm != null && !string.IsNullOrEmpty(panelFilter.SearchTerm))
            {
                whereQuery += $"and ( e.EmployeeName like '%{panelFilter.SearchTerm}%' or e.GlobantEmailAddress  like '%{panelFilter.SearchTerm}%' ) ";
            }

            sortQuery = $" order by EmployeeName asc ";

            string finalquery = basequery + whereQuery + sortQuery;

            DataTable dataTableInterviewPanelDetails = await ExecuteStoredProcedure(finalquery);

            // Ensure the DataTable has data to process
            if (dataTableInterviewPanelDetails == null || dataTableInterviewPanelDetails.Rows.Count == 0)
            {
                return data; // Return an empty list
            }

            // Get the comma-separated string from the specified column
            string PanelIdcsv = GetCommaSeparatedString(dataTableInterviewPanelDetails, "PanelId");

            string whereQueryslots = $" where ps.SlotDate between '{formattedDateTimeStart}' and '{formattedDateTimeEnd}' ";

            if (!string.IsNullOrEmpty(PanelIdcsv))
            {
                whereQueryslots += $"  and  PanelId in ({PanelIdcsv})";
            }

            string finalquerySlots = "select Id,PanelId,SlotDate,IsUtilized from [dbo].[PanelSlots] ps " + whereQueryslots;

            DataTable dataTablePanelSlots = await ExecuteStoredProcedure(finalquerySlots);


            List<InterviewScheduleData> InterviewScheduleData = new List<InterviewScheduleData>();

            foreach (DataRow row in dataTableInterviewPanelDetails.Rows)
            {
                var SlotsList = (from DataRow rowdata in dataTablePanelSlots.AsEnumerable()
                                 where rowdata.Field<string>("PanelId") == Convert.ToString(row["PanelId"])
                                 select new AllPanelSlots
                                 {
                                     Id = Convert.ToInt32(rowdata.Field<string>("Id")),
                                     SlotDate = Convert.ToDateTime(rowdata.Field<string>("SlotDate")),
                                     IsUtilized = Convert.ToBoolean(rowdata.Field<string>("IsUtilized"))

                                 }).ToList();


                InterviewScheduleData interviewScheduleData = new InterviewScheduleData
                {
                    PanelId = Convert.ToInt32(row["PanelId"]),
                    EmailId = Convert.ToString(row["EmailId"]),
                    Panel = Convert.ToString(row["Panel"]),
                    PrimaryPanel = Convert.ToString(row["PrimaryPanel"]),
                    UpToSeniority = Convert.ToString(row["UpToSeniority"]),
                    CommunityName = Convert.ToString(row["CommunityName"]),
                    Slots = SlotsList
                };

                InterviewScheduleData.Add(interviewScheduleData);
            }

            data.InterviewScheduleData = InterviewScheduleData;

            return data;
        }

        static string GetCommaSeparatedString(DataTable dataTable, string columnName)
        {
            // Use LINQ to select the specified column and join the values
            return string.Join(", ", dataTable.AsEnumerable()
                                               .Select(row => row.Field<string>(columnName)));
        }



        public async Task<string> SavePanelSlotCalenderEvent(PanelSlotsCalenderEvent panelSlotsCalenderEvent)
        {
            string result = "true";
            try
            {

                object[] keyValues = new object[] { panelSlotsCalenderEvent.Id };
                var panelSlot = _panelSlots.Find(keyValues);

                if (panelSlot == null)
                {
                    return "false";
                }

                //TimeZoneInfo sourceTimezone = TimeZoneInfo.FindSystemTimeZoneById(panelSlotsCalenderEvent.TargetIanaTimeZoneId);

                string localDateTimeString = panelSlotsCalenderEvent.SlotDate;
                DateTime localDateTime = DateTime.Parse(localDateTimeString);

                // Create new DateTime with Unspecified kind
                //DateTime unspecifiedDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

                // Now convert properly
                DateTime utcSlotDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime);

                string CalenderEventIDExisting = panelSlot.CalenderEventID;
                var CalenderEventID = await GetCalenderEventID(panelSlotsCalenderEvent, CalenderEventIDExisting);

                panelSlot.IsUtilized = true;
                panelSlot.CalenderEventID = CalenderEventID.ToString();
                panelSlot.CandidateEmail = panelSlotsCalenderEvent.CandidateEmail;
                panelSlot.FileEncoded = panelSlotsCalenderEvent.FileEncoded;
                panelSlot.LoggedinUserEmailId = panelSlotsCalenderEvent.LoggedInUserEmailID;
                panelSlot.ResumeFileName = panelSlotsCalenderEvent.ResumeFileName;
                panelSlot.EventTitle = panelSlotsCalenderEvent.EventTitle;
                panelSlot.SlotDate = utcSlotDateTime;
                panelSlot.Recruiter = panelSlotsCalenderEvent.Recruiter;
                panelSlot.UpdatedOn = DateTime.UtcNow;
                panelSlot.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                _panelSlots.Update(panelSlot);
                _unitOfWork.SaveChanges();

                return "true";

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private async Task<string> GetCalenderEventID(PanelSlotsCalenderEvent panelSlotsCalenderEvent, string CalenderEventIDExisting)
        {
            var CalenderEventID = "";

            try
            {
                string serviceAccountEmail = "dal.automations@globant.com";

                string eventDescription = string.Empty;

                string filestring = panelSlotsCalenderEvent.FileEncoded;
                // Convert Base64 string to byte array

                string fileName = panelSlotsCalenderEvent.ResumeFileName; // Name of the file to be uploaded
                string ApplicationName = "DAS-AGK-Interview-Panel";
                string Recruiter = panelSlotsCalenderEvent.Recruiter;
                List<string> RecruiterList = Recruiter.Split(',').Select(s => s.Trim()).ToList();
                RecruiterList.Add(panelSlotsCalenderEvent.LoggedInUserEmailID);

                // Authenticate and create Google Calendar API service
                UserCredential credential = await Authenticate(serviceAccountEmail);

                // Upload the file to Google Drive and get the public URL
                if (!string.IsNullOrEmpty(filestring))
                {
                    byte[] fileBytes = Convert.FromBase64String(filestring);
                    string fileUrl = await UploadFileToDrive(credential, ApplicationName, fileBytes, fileName, serviceAccountEmail, RecruiterList);
                    eventDescription = "<br/> <br/>Candidate profile link - <br/>" + fileUrl + "<br/><br/>";
                }

                var calendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                //attendees list
                //attendees list
                List<EventAttendee> attendees = new List<EventAttendee>
            {
                new EventAttendee() { Email = panelSlotsCalenderEvent.CandidateEmail },
            };

                foreach (var item in RecruiterList)
                {
                    attendees.Add(new EventAttendee() { Email = item });
                }

                EventAttendee[] attendeesArray = attendees.ToArray();


                TimeZoneInfo targetZone = GetTimeZoneInfo(panelSlotsCalenderEvent.TargetIanaTimeZoneId);

                DateTime localStartTime = DateTime.Parse(panelSlotsCalenderEvent.SlotDate);
                DateTime localEndTime = localStartTime.AddHours(1);

                // Ensure the input DateTime Kind is Unspecified before calculating offset
                // This prevents misinterpretation if the input DateTime was accidentally created as Local or Utc
                var startTimeUnspecified = DateTime.SpecifyKind(localStartTime, DateTimeKind.Unspecified);
                var endTimeUnspecified = DateTime.SpecifyKind(localEndTime, DateTimeKind.Unspecified);

                // Calculate the specific UTC offset for the given local time IN THAT ZONE
                // This correctly handles Daylight Saving Time transitions
                TimeSpan startOffset = targetZone.GetUtcOffset(startTimeUnspecified);
                TimeSpan endOffset = targetZone.GetUtcOffset(endTimeUnspecified);
                // TODO: Add checks for ambiguous/invalid times if needed using targetZone.IsAmbiguousTime / IsInvalidTime

                // Create the precise DateTimeOffset representing the exact point in time
                DateTimeOffset startSpecificTime = new DateTimeOffset(startTimeUnspecified, startOffset);
                DateTimeOffset endSpecificTime = new DateTimeOffset(endTimeUnspecified, endOffset);

                // Create a new event
                Event newEvent = new Event()
                {

                    Organizer = new OrganizerData()
                    {
                        Email = "Globanr Recruitment", // The email of the organizer
                        DisplayName = serviceAccountEmail
                    },

                    Summary = panelSlotsCalenderEvent.EventTitle,
                    Location = "Google Meet",
                    Description = panelSlotsCalenderEvent.EventTitle + eventDescription,
                    Start = new EventDateTime()
                    {
                        // Provide the exact point in time using DateTimeOffset
                        DateTimeDateTimeOffset = startSpecificTime,
                        // **Crucially, also provide the IANA Time Zone ID**
                        // This tells Google Calendar how to interpret this time, handle potential DST,
                        // and display it correctly to users in different zones.
                        TimeZone = panelSlotsCalenderEvent.TargetIanaTimeZoneId
                    },
                    End = new EventDateTime()
                    {
                        DateTimeDateTimeOffset = endSpecificTime,
                        TimeZone = panelSlotsCalenderEvent.TargetIanaTimeZoneId
                    },
                    ConferenceData = new ConferenceData()
                    {
                        CreateRequest = new CreateConferenceRequest()
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            ConferenceSolutionKey = new ConferenceSolutionKey()
                            {
                                Type = "hangoutsMeet"
                            }
                        }
                    },
                    Attendees = attendeesArray
                };


                //delete existing event - case in case calender event date time change
                if (!string.IsNullOrEmpty(CalenderEventIDExisting))
                {
                    var requestUpdate = calendarService.Events.Update(newEvent, "primary", CalenderEventIDExisting);
                    requestUpdate.ConferenceDataVersion = 1; // Set the conference data version
                    requestUpdate.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All; // Send notifications to all attendees
                    newEvent = await requestUpdate.ExecuteAsync();
                    CalenderEventID = newEvent.Id;
                }
                else
                {
                    // Insert the event into the calendar with email notifications
                    var request = calendarService.Events.Insert(newEvent, "primary");
                    request.ConferenceDataVersion = 1; // Set the conference data version
                    request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All; // Send notifications to all attendees
                    newEvent = await request.ExecuteAsync();
                    CalenderEventID = newEvent.Id;
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return CalenderEventID;
        }

        private TimeZoneInfo GetTimeZoneInfo(string targetIanaTimeZoneId)
        {
            TimeZoneInfo targetZone;

            try
            {
                // TZConvert.GetTimeZoneInfo will find the appropriate TimeZoneInfo
                // object regardless of whether the code is running on Windows or Linux/macOS.
                targetZone = TZConvert.GetTimeZoneInfo(targetIanaTimeZoneId);

            }
            catch (TimeZoneNotFoundException ex)
            {
                // Basic fallback attempt for Windows if IANA fails (less reliable)
                if (OperatingSystem.IsWindows() && TimeZoneInfo.TryConvertIanaIdToWindowsId(targetIanaTimeZoneId, out string windowsId))
                {
                    targetZone = TimeZoneInfo.FindSystemTimeZoneById(windowsId);
                }
                else
                {
                    throw ex; // Re-throw original exception if no mapping found
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return targetZone;
        }

        private async Task<string> GetOrCreateFolder(DriveService service, string folderName)
        {
            // Search for the folder
            string query = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
            var request = service.Files.List();
            request.Q = query;
            request.Fields = "files(id, name)";
            var result = await request.ExecuteAsync();

            // If the folder exists, return its ID
            if (result.Files.Count > 0)
            {
                Console.WriteLine($"Folder '{folderName}' already exists with ID: {result.Files[0].Id}");
                return result.Files[0].Id;
            }
            else
            {
                // Create the folder
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                var createRequest = service.Files.Create(folderMetadata);
                createRequest.Fields = "id";
                var folder = await createRequest.ExecuteAsync();
                Console.WriteLine($"Created folder '{folderName}' with ID: {folder.Id}");
                return folder.Id;
            }
        }

        private async Task<string> UploadFileToDrive(UserCredential credential, string ApplicationName, byte[] fileBytes, string fileName, string serviceAccountEmail, List<string> RecruiterList)
        {
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            // Get or create the folder
            string folderId = await GetOrCreateFolder(driveService, "AGK-InterviewPanelProfiles");


            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { folderId },
                MimeType = "application/pdf" // Specify the correct MIME type
            };

            using (var stream = new MemoryStream(fileBytes))
            {
                // Set the position of the stream to the beginning
                stream.Position = 0;

                var request = driveService.Files.Create(fileMetadata, stream, "application/pdf");
                request.Fields = "id";
                var file = await request.UploadAsync();

                if (file.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new Exception("Failed to upload file to Drive.");
                }

                // Get the file ID and create a shareable link
                var uploadedFile = request.ResponseBody;

                string filepath = $"https://drive.google.com/file/d/{uploadedFile.Id}/view?usp=drive_link";// Public URL format

                //share file with Recruiters
                foreach (var item in RecruiterList)
                {

                    Permission permission = new Permission()
                    {
                        Type = "user",
                        Role = "reader", // Can be "reader", "writer", or "owner"
                        EmailAddress = item
                    };

                    var requestFileShare = driveService.Permissions.Create(permission, uploadedFile.Id);
                    requestFileShare.SendNotificationEmail = false; // Disable email notification
                    requestFileShare.Execute();
                }

                return filepath;
            }
        }

        private async Task<UserCredential> Authenticate(string serviceAccountEmail)
        {
            string clientIdEncrypted = _appSetting.GoogleCalender.ClientId;
            string clientSecretEncrypted = _appSetting.GoogleCalender.ClientSecret;
            string keyEnDecrypt = _appSetting.GoogleCalender.KeyEnDecrypt;
            string clientId = Decrypt(clientIdEncrypted, keyEnDecrypt);
            string clientSecret = Decrypt(clientSecretEncrypted, keyEnDecrypt);
            UserCredential credential = null;

            try
            {
                string[] combinedScopes = new[]
                   {
                CalendarService.Scope.Calendar,
                DriveService.Scope.Drive // This provides full access to Drive
            };

                var panelUserCredentialFromDB = await _panelUserCredential.GetFirstOrDefaultAsync(
                                                                                selector: s => new { s.AccessToken, s.RefreshToken, s.ExpiryTime, s.UserId, s.Id, s.CreatedDate },
                                                                                predicate: s => s.UserId == serviceAccountEmail
                                                                            );

                if (panelUserCredentialFromDB != null && panelUserCredentialFromDB.RefreshToken != null) //update record
                {
                    TokenResponse tokenResponse = new TokenResponse();

                    tokenResponse.AccessToken = panelUserCredentialFromDB.AccessToken;
                    tokenResponse.RefreshToken = panelUserCredentialFromDB.RefreshToken;


                    // Use stored tokens
                    credential = new UserCredential(new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret,
                        }
                    }), serviceAccountEmail, tokenResponse);

                    DateTime? tokenExpiration = panelUserCredentialFromDB.ExpiryTime; // Expiration time


                    // Check if the access token is expired and refresh it if necessary
                    if (tokenExpiration.HasValue && tokenExpiration.Value <= DateTime.UtcNow)
                    {
                        // Refresh the access token
                        await credential.RefreshTokenAsync(CancellationToken.None);
                        // Update the new access token in your database

                        var panelUserCredential = new PanelUserCredential
                        {
                            Id = panelUserCredentialFromDB.Id,
                            UserId = panelUserCredentialFromDB.UserId,
                            AccessToken = credential.Token.AccessToken,
                            RefreshToken = credential.Token.RefreshToken,
                            ExpiryTime = DateTime.UtcNow.AddSeconds((double)credential.Token.ExpiresInSeconds),
                            CreatedDate = DateTime.UtcNow
                        };

                        _panelUserCredential.Update(panelUserCredential);
                        await _unitOfWork.SaveChangesAsync();


                    }
                }
                else //insert record
                {
                    //C:\ProgramData\AGK-InterviewPanel\dal.automations@globant.com
                    string commonAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    string myAppDataPath = Path.Combine(commonAppDataPath, "AGK-InterviewPanel", serviceAccountEmail);


                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                              new ClientSecrets
                                              {
                                                  ClientId = clientId,
                                                  ClientSecret = clientSecret
                                              },
                                              combinedScopes, // Make sure this includes Drive scopes when uploading
                                              serviceAccountEmail,
                                              CancellationToken.None,
                                              new FileDataStore(myAppDataPath, true)
                                          );

                    var panelUserCredential = new PanelUserCredential
                    {
                        UserId = serviceAccountEmail,
                        AccessToken = credential.Token.AccessToken,
                        RefreshToken = credential.Token.RefreshToken,
                    };

                    await _panelUserCredential.InsertAsync(panelUserCredential);
                    await _unitOfWork.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
            return credential;
        }

        public static string Decrypt(string cipherText, string key)
        {
            byte[] iv = new byte[16]; // Initialization vector (IV)
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }


        public async Task<PanelSlotDetailModel> GetPanelSlotDataById(int slotId)
        {
            var panelSlotEntity = _panelSlots.GetAll().SingleOrDefault(x => x.Id == slotId);

            if (panelSlotEntity != null)
            {
                var result = new PanelSlotDetailModel
                {
                    SlotDate = panelSlotEntity.SlotDate,
                    Recruiter = panelSlotEntity.Recruiter,
                    CandidateName = panelSlotEntity.CandidateName,
                    CandidateEmail = panelSlotEntity.CandidateEmail,
                    FileEncoded = panelSlotEntity.FileEncoded,
                    LoggedInUserEmailID = panelSlotEntity.LoggedinUserEmailId,
                    ResumeFileName = panelSlotEntity.ResumeFileName,
                    EventTitle = panelSlotEntity.EventTitle
                };

                return result;
            }

            return null;
        }

        public async Task<List<AIEvaluationDataModel>> GetAIEvaluation(string panelEmail)
        {
            AIEvaluationDataModel data = new AIEvaluationDataModel();
            List<AIEvaluationDataModel> listData = new List<AIEvaluationDataModel>();

            try
            {

                string basequery = string.Empty;
                basequery = "SELECT top 20 AIEvaluationStatus, AIEvaluationStatusComment, Strengths, AreasForImprovement, OverallEvaluation, FinalRatingInNumber, FinalRatingInText,ModifiedDate,PanelName,PanelEmail,InterviewDate,InterviewName, Interviewer, MainStream, StreamsCovered, TotalQuestionsAsked, TargetExperienceTier, CandidateExperienceYears, QuestionsAskedByPanel, MaxRatingValue  FROM dbo.PanelAIEvaluation WHERE PanelEmail = '" + panelEmail + "' AND AIEvaluationStatus = '" + InterviewContants.AIEvaluationStatus + "' order by ModifiedDate desc ";
                DataTable dataTableAIEvaluation = await ExecuteStoredProcedure(basequery);
                foreach (DataRow row in dataTableAIEvaluation.Rows)
                {
                    AIEvaluationDataModel objEvaluation = new AIEvaluationDataModel();
                    objEvaluation.AIEvaluationStatus = row["AIEvaluationStatus"] != null ? Convert.ToString(row["AIEvaluationStatus"]) : "";
                    objEvaluation.AIEvaluationStatusComment = row["AIEvaluationStatusComment"] != null ? Convert.ToString(row["AIEvaluationStatusComment"]) : "";
                    objEvaluation.Strengths = row["Strengths"] != null ? Convert.ToString(row["Strengths"]) : ""; 
                    objEvaluation.AreasForImprovement = row["AreasForImprovement"] != null ? Convert.ToString(row["AreasForImprovement"]) : "";
                    objEvaluation.OverallEvaluation = row["OverallEvaluation"] != null ? Convert.ToString(row["OverallEvaluation"]) : "";
                    objEvaluation.FinalRatingInNumber = row["FinalRatingInNumber"] != "" ? Convert.ToDecimal(row["FinalRatingInNumber"]) : 0;
                    objEvaluation.FinalRatingInText = row["FinalRatingInText"] != null ? Convert.ToString(row["FinalRatingInText"]) : "";
                    objEvaluation.ModifiedDate = row["ModifiedDate"] != null ? Convert.ToDateTime(row["ModifiedDate"]) : DateTime.MinValue;
                    objEvaluation.PanelName = row["PanelName"] != null ? Convert.ToString(row["PanelName"]) : "";
                    objEvaluation.PanelEmail = row["PanelEmail"] != null ? Convert.ToString(row["PanelEmail"]) : "";
                    objEvaluation.InterviewDate = row["InterviewDate"] != null ? Convert.ToDateTime(row["InterviewDate"]) : DateTime.MinValue;
                    objEvaluation.InterviewName = row["InterviewName"] != null ? Convert.ToString(row["InterviewName"]) : "";

                    objEvaluation.Interviewer = row["Interviewer"] != null ? Convert.ToString(row["Interviewer"]) : "";
                    objEvaluation.MainStream = row["MainStream"] != null ? Convert.ToString(row["MainStream"]) : "";
                    objEvaluation.StreamsCovered = row["StreamsCovered"] != null ? Convert.ToString(row["StreamsCovered"]) : "";
                    objEvaluation.TotalQuestionsAsked = row["TotalQuestionsAsked"] != "" ? Convert.ToInt32(row["TotalQuestionsAsked"]) : 0;
                    objEvaluation.TargetExperienceTier = row["TargetExperienceTier"] != null ? Convert.ToString(row["TargetExperienceTier"]) : "";
                    objEvaluation.CandidateExperienceYears = row["CandidateExperienceYears"] != null ? Convert.ToString(row["CandidateExperienceYears"]) : "";
                    objEvaluation.QuestionsAskedByPanel = row["QuestionsAskedByPanel"] != null ? Convert.ToString(row["QuestionsAskedByPanel"]) : "";
                    objEvaluation.MaxRatingValue = row["MaxRatingValue"] != null ? Convert.ToString(row["MaxRatingValue"]) : "";
                    listData.Add(objEvaluation);
                }

            }
            catch (Exception ex)
            {
               // _logger.LogInformation("GetAIEvaluation error " + ex);
            }

            return listData;
        }
    }
}
