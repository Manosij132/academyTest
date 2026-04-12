using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Arch.EntityFrameworkCore.UnitOfWork;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Academy.Core.Services
{
    //AGK API Migration
    public class CandidateProfileService : ICandidateProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IRepository<Employee> _employee;

        public CandidateProfileService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _config = config;
            _unitOfWork = unitOfWork;
            _employee = _unitOfWork.GetRepository<Employee>();
        }

        public List<PanelEfficiencyDto> ConvertToDTOs(IList<IList<object>> lstCandidateDetails)
        {
            List<PanelEfficiencyDto> candidates = new List<PanelEfficiencyDto>();

            // Get the column names from the first row
            IList<object> columnNames = lstCandidateDetails[0];
            // Assuming the order of properties in the DTO matches the column order
            int l1PanelNameIndex = columnNames.IndexOf("L1 Panel(1)");
            int gkPanelNameIndex = columnNames.IndexOf("GK Panel");
            int statusIndex = columnNames.IndexOf("Status");
            int l1DateIndex = columnNames.IndexOf("L1 Date");
            int gKDateIndex = columnNames.IndexOf("GK Date");
            int l1SeniorityIndex = columnNames.IndexOf("L1 Panels Seniority");
            int gKSeniorityIndex = columnNames.IndexOf("GK Panels Seniority");

            for (int i = 1; i < lstCandidateDetails.Count; i++)
            {
                try
                {
                    // Check if the inner list has enough elements
                    if (lstCandidateDetails[i].Count > Math.Max(l1PanelNameIndex, Math.Max(statusIndex, Math.Max(gkPanelNameIndex, Math.Max(l1DateIndex, gKDateIndex)))))
                    {
                        string isPanelNameEmpty = lstCandidateDetails[i][l1PanelNameIndex].ToString();
                        string status = lstCandidateDetails[i][statusIndex]?.ToString();
                        string gkPanelName = lstCandidateDetails[i][gkPanelNameIndex]?.ToString().ToLower();
                        string startDate = lstCandidateDetails[i][l1DateIndex]?.ToString();
                        string endDate = lstCandidateDetails[i][gKDateIndex]?.ToString();
                        string l1Seniority = lstCandidateDetails[i][l1SeniorityIndex]?.ToString();
                        string gKSeniority = lstCandidateDetails[i][gKSeniorityIndex]?.ToString();

                        if (IgnoreNotRequiredCandidates(status) && !string.IsNullOrEmpty(isPanelNameEmpty) && isPanelNameEmpty != "backup")
                        {
                            PanelEfficiencyDto candidate = new PanelEfficiencyDto
                            {
                                PanelName = isPanelNameEmpty.ToLower(),
                                GKPanelName = gkPanelName,
                                StartDate = startDate,
                                EndDate = endDate,
                                L1Seniority = l1Seniority,
                                GKSeniority = gKSeniority
                            };

                            // Set PanelType 
                            SetRequiredDetails(status, candidate);
                            SetPanelType(candidate);

                            // Add the DTO object to the list
                            candidates.Add(candidate);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error on iteration {i}: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    Console.WriteLine($"Data: {String.Join(", ", lstCandidateDetails[i])}");
                }
            }

            return candidates;
        }

        private void SetPanelType(PanelEfficiencyDto panelEfficiencyDto)
        {
            switch (panelEfficiencyDto.GKPanelName)
            {
                case "":
                    panelEfficiencyDto.PanelType = "L1";
                    break;
                default:
                    panelEfficiencyDto.PanelType = "GK";
                    break;
            }
        }

        private void SetRequiredDetails(string status, PanelEfficiencyDto panelEfficiencyDto)
        {
            switch (status)
            {
                case "CNO":
                case "Joined":
                case "Lost Due to Account Delay":
                case "Candidate Withdrew after GK Interview":
                case "Offer Declined":
                case "Client Interview Rejected":
                case "Cancel Hire":
                case "Offer Accepted":
                case "Account Interview Reject":
                case "Client Presented":
                case "Lost Due to Client Delay":
                case "Account Screening Reject":
                case "Candidate Withdrew after Client Interview":
                case "Offer in Discussion":
                case "GK Select":
                case "Offer Made":
                    panelEfficiencyDto.L1Conducted = 1;
                    panelEfficiencyDto.L1Selected = 1;
                    panelEfficiencyDto.GKConducted = 1;
                    panelEfficiencyDto.GKSelected = 1;
                    break;
                case "GK Reject":
                    panelEfficiencyDto.L1Conducted = 1;
                    panelEfficiencyDto.L1Selected = 1;
                    panelEfficiencyDto.GKConducted = 1;
                    panelEfficiencyDto.GKSelected = 0;
                    break;
                case "L1+GK Select":
                    panelEfficiencyDto.L1Conducted = 0;
                    panelEfficiencyDto.L1Selected = 0;
                    panelEfficiencyDto.GKConducted = 1;
                    panelEfficiencyDto.GKSelected = 1;
                    break;
                case "L1+GK Reject":
                    panelEfficiencyDto.L1Conducted = 0;
                    panelEfficiencyDto.L1Selected = 0;
                    panelEfficiencyDto.GKConducted = 1;
                    panelEfficiencyDto.GKSelected = 0;
                    break;
                case "L1 Reject":
                    panelEfficiencyDto.L1Conducted = 1;
                    panelEfficiencyDto.L1Selected = 0;
                    panelEfficiencyDto.GKConducted = 0;
                    panelEfficiencyDto.GKSelected = 0;
                    break;
                case "Candidate Withdrew after L1 Interview":
                case "GK Scheduled":
                case "L1 Select":
                case "Position Onhold/Cancelled after L1 Interview":
                    panelEfficiencyDto.L1Conducted = 1;
                    panelEfficiencyDto.L1Selected = 1;
                    panelEfficiencyDto.GKConducted = 0;
                    panelEfficiencyDto.GKSelected = 0;
                    break;
                default:
                    break;
            }
        }

        private bool IgnoreNotRequiredCandidates(string status)
        {
            switch (status)
            {
                case "Candidate Withdrew after Sourcing":
                case "HR Screening Reject":
                case "L1 Scheduled":
                case "Sourced":
                case "L1+GK Scheduled":
                case "HR Screening Select":
                    return false;
                default:
                    return true;
            }
        }

        private List<PanelEfficiencyDto> GetL1PanelDataForEfficiency(List<PanelEfficiencyDto> panelEfficiencyDtos)
        {
            List<PanelEfficiencyDto> SortedL1List = panelEfficiencyDtos.Where(x => x.PanelName != x.GKPanelName).ToList();

            var GroupedL1Data = SortedL1List
                             .GroupBy(x => x.PanelName)
                             .Select(group => new PanelEfficiencyDto
                             {
                                 PanelName = group.Key,
                                 PanelType = "L1",
                                 L1Conducted = group.Sum(item => item.L1Conducted),
                                 L1Selected = group.Sum(item => item.L1Selected),
                                 GKConducted = group.Sum(item => item.GKConducted),
                                 GKSelected = group.Sum(item => item.GKSelected),
                                 CountwiseEfficiency = Math.Round(((double)group.Where(x => x.L1Selected == 1).Count() / group.Count() * 100), 2),
                                 Efficiency = double.IsNaN(Math.Round(((double)group.Sum(item => item.GKSelected) / group.Sum(item => item.GKConducted) * 100), 2)) ? -1 : Math.Round(((double)group.Sum(item => item.GKSelected) / group.Sum(item => item.GKConducted) * 100), 2),
                                 L1Seniority = group.Select(item => item.L1Seniority).FirstOrDefault(seniority => !string.IsNullOrEmpty(seniority)),
                                 GKSeniority = group.Select(item => item.GKSeniority).FirstOrDefault(seniority => !string.IsNullOrEmpty(seniority))
                             }).ToList();
            return GroupedL1Data;
        }

        private List<PanelEfficiencyDto> GetGKPanelDataForEfficiency(List<PanelEfficiencyDto> panelEfficiencyDtos)
        {
            List<PanelEfficiencyDto> SortedL1List = panelEfficiencyDtos.Where(x => x.PanelType != "L1").ToList();

            var GroupedL1Data = SortedL1List
                             .GroupBy(x => x.GKPanelName)
                             .Select(group => new PanelEfficiencyDto
                             {
                                 PanelName = group.Key,
                                 PanelType = "GK",
                                 L1Conducted = 0,
                                 L1Selected = 0,
                                 GKConducted = group.Sum(item => item.GKConducted),
                                 GKSelected = group.Sum(item => item.GKSelected),
                                 CountwiseEfficiency = 0,
                                 Efficiency = Math.Round(((double)group.Sum(item => item.GKSelected) / group.Sum(item => item.GKConducted) * 100), 2),
                                 L1Seniority = group.Select(item => item.L1Seniority).FirstOrDefault(seniority => !string.IsNullOrEmpty(seniority)),
                                 GKSeniority = group.Select(item => item.GKSeniority).FirstOrDefault(seniority => !string.IsNullOrEmpty(seniority))
                             }).ToList();
            return GroupedL1Data;
        }

        public List<PanelEfficiencyResponseDto> Process(int pageNumber, int pageSize, string? startDate, string? endDate)
        {
            IList<IList<object>> dataFromSourceSheet = ReadDataFromSpreadSheet(true, _config["PanelEfficiency:sheetUrlKeyEffi"], _config["PanelEfficiency:sheeDummyForAll"], "A:Z");

            List<PanelEfficiencyDto> candidateData = ConvertToDTOs(dataFromSourceSheet);

            DateTime endDateTime = string.IsNullOrEmpty(endDate) ? DateTime.Today : DateTime.Parse(endDate);
            List<PanelEfficiencyDto> filteredCandidates;

            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime startDateTime = DateTime.Parse(startDate);
                // Filter candidates based on L1 Date column
                filteredCandidates = candidateData
                 .Where(c => IsValidDate(c.StartDate) &&
                             DateTime.Parse(c.StartDate) >= startDateTime &&
                             DateTime.Parse(c.StartDate) <= endDateTime)
                 .ToList();
            }
            else
            {
                // Use all data if no start date is provided
                filteredCandidates = candidateData;
            }

            List<PanelEfficiencyDto> L1SortedEffiData = GetL1PanelDataForEfficiency(filteredCandidates);
            List<PanelEfficiencyDto> GKSortedEffiData = GetGKPanelDataForEfficiency(filteredCandidates);

            L1SortedEffiData = L1SortedEffiData.OrderBy(x => x.PanelName).ToList();
            GKSortedEffiData = GKSortedEffiData.OrderBy(x => x.PanelName).ToList();

            List<PanelEfficiencyResponseDto> L1GKMergedData = L1SortedEffiData.Concat(GKSortedEffiData)
               .Select(item => new PanelEfficiencyResponseDto
               {
                   PanelName = item.PanelName,
                   PanelType = item.PanelType,
                   L1Conducted = item.L1Conducted,
                   L1Selected = item.L1Selected,
                   GKConducted = item.GKConducted,
                   GKSelected = item.GKSelected,
                   Efficiency = double.IsNaN(item.Efficiency) ? 0 : item.Efficiency,
                   CountwiseEfficiency = item.CountwiseEfficiency,
                   TDC = string.Empty,
                   Community = string.Empty,
                   Seniority = item.PanelType == "L1" ? item.L1Seniority : item.GKSeniority
               })
               .ToList();

            L1GKMergedData = this.GetTDCCommunity(L1GKMergedData);

            return L1GKMergedData;
        }

        private List<PanelEfficiencyResponseDto> GetTDCCommunity(List<PanelEfficiencyResponseDto> L1GKMergedData)
        {
            try
            {
                foreach (var panel in L1GKMergedData)
                {
                    var employee = _employee.GetAll().Where(e => e.GlobantEmailAddress == panel.PanelName).FirstOrDefault();
                    if (employee != null)
                    {
                        panel.TDC = employee.Tdc;
                        panel.Community = employee.Community;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                return new List<PanelEfficiencyResponseDto>(); // Return an empty list in case of an error
            }

            return L1GKMergedData;
        }

        private bool IsValidDate(string dateString)
        {
            return DateTime.TryParse(dateString, out _);
        }

        public int GetTotalCount(string? startDate, string? endDate)
        {
            IList<IList<object>> dataFromSourceSheet = ReadDataFromSpreadSheet(true, _config["PanelEfficiency:sheetUrlKeyEffi"], _config["PanelEfficiency:sheeDummyForAll"], "A:Z");

            List<PanelEfficiencyDto> candidateData = ConvertToDTOs(dataFromSourceSheet);

            DateTime endDateTime = string.IsNullOrEmpty(endDate) ? DateTime.Today : DateTime.Parse(endDate);
            List<PanelEfficiencyDto> filteredCandidates;

            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime startDateTime = DateTime.Parse(startDate);
                // Filter candidates based on L1 Date column
                filteredCandidates = candidateData
                 .Where(c => IsValidDate(c.StartDate) &&
                             DateTime.Parse(c.StartDate) >= startDateTime &&
                             DateTime.Parse(c.StartDate) <= endDateTime)
                 .ToList();
            }
            else
            {
                // Use all data if no start date is provided
                filteredCandidates = candidateData;
            }

            List<PanelEfficiencyDto> L1SortedEffiData = GetL1PanelDataForEfficiency(filteredCandidates);
            List<PanelEfficiencyDto> GKSortedEffiData = GetGKPanelDataForEfficiency(filteredCandidates);

            List<PanelEfficiencyResponseDto> L1GKMergedData = L1SortedEffiData.Concat(GKSortedEffiData)
               .Select(item => new PanelEfficiencyResponseDto
               {
                   PanelName = item.PanelName,
                   PanelType = item.PanelType,
                   L1Conducted = item.L1Conducted,
                   L1Selected = item.L1Selected,
                   GKConducted = item.GKConducted,
                   GKSelected = item.GKSelected,
                   Efficiency = double.IsNaN(item.Efficiency) ? 0 : item.Efficiency,
                   CountwiseEfficiency = item.CountwiseEfficiency
               })
               .ToList();

            return L1GKMergedData.Count;
        }

        private IList<IList<object>> ReadDataFromSpreadSheet(bool isHeaderPresent, string sheetId, string sheetName, string sheetRange)
        {
            string[] SheetScopes = { SheetsService.Scope.Spreadsheets };
            var sheetCredentialsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config["PanelEfficiency:SheetCredentialsFile"]);

            var googleCredentials = GoogleCredential.FromFile(
                sheetCredentialsFile)
                .CreateScoped(SheetScopes);

            var assemblyProduct = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyProductAttribute>();

            var _sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                ApplicationName = assemblyProduct.Product,
                HttpClientInitializer = googleCredentials,
            });
            // Specifying Column Range for reading...
            var range = $"{sheetName}!{sheetRange}";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetService.Spreadsheets.Values.Get(sheetId, range);

            // Executing Read Operation...
            var response = request.Execute();
            // Getting all records from Column A to E...
            IList<IList<object>> values = response.Values;

            if (isHeaderPresent)
            {
                return values;
            }
            else
            {
                values.RemoveAt(0);
                return values;
            }

        }
    }
}