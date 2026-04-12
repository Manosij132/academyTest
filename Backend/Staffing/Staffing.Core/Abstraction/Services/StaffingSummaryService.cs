using Microsoft.Data.SqlClient;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using System.Data;

namespace Staffing.Core.Abstraction.Services
{
    public class StaffingSummaryService : IStaffingSummaryService
    {
        private const int DefaultTimeout = 120;

        #region Summary

        public async Task<SummaryResponse> GetSummaryAsync(
            DataConnection dbConnection,
            DateTime? startDate,
            DateTime? endDate)
        {
            var response = new SummaryResponse();

            using var conn = new SqlConnection(dbConnection.ConnectionString);
            await conn.OpenAsync();

            // 1️⃣ AIStudioGroup
            await using (var cmd = CreateCommand(conn, "dbo.sp_GetAIStudioGroupSummary"))
            {
                AddDateParameters(cmd, startDate, endDate);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    response.AIStudioGroups.Add(new GroupCount
                    {
                        Id = reader.GetInt32(0),
                        GroupName = reader.GetString(1),
                        GroupNameCount = reader.GetInt32(2)
                    });
                }
            }

            // 2️⃣ Client
            await using (var cmd = CreateCommand(conn, "dbo.sp_GetClientSummary"))
            {
                AddDateParameters(cmd, startDate, endDate);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    response.Clients.Add(new ClientCount
                    {
                        Id = reader.GetInt32(0),
                        Client = reader.GetString(1),
                        ClientCountValue = reader.GetInt32(2)
                    });
                }
            }

            // 3️⃣ Detailed Status
            await using (var cmd = CreateCommand(conn, "dbo.sp_GetDetailedStatusSummary"))
            {
                AddDateParameters(cmd, startDate, endDate);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    response.DetailedStatuses.Add(new StatusCount
                    {
                        Id = reader.GetInt32(0),
                        StatusName = reader.GetString(1),
                        StatusNameCount = reader.GetInt32(2)
                    });
                }
            }

            return response;
        }

        #endregion

        #region Ticket Filtering

        public async Task<(List<TicketFilteredData>, long)> GetTicketFilteredDataAsync(
            DataConnection dbConnection,
            GetFilteredTicketDataRequest request)
        {
            var tickets = new List<TicketFilteredData>();
            int totalCount = 0;

            await using var conn = new SqlConnection(dbConnection.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.sp_GetTicketFilteredData", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@GroupName", SqlDbType.NVarChar).Value =
                (object?)ToPipeSeparated(request.GroupNames) ?? DBNull.Value;

            cmd.Parameters.Add("@Client", SqlDbType.NVarChar).Value =
                (object?)ToPipeSeparated(request.Client) ?? DBNull.Value;

            cmd.Parameters.Add("@DetailedStatus", SqlDbType.NVarChar).Value =
                (object?)ToPipeSeparated(request.DetailedStatuses) ?? DBNull.Value;

            cmd.Parameters.Add("@TicketStatus", SqlDbType.NVarChar).Value =
                (object?)ToPipeSeparated(request.TicketStatus) ?? DBNull.Value;

            cmd.Parameters.Add("@MonthClosure", SqlDbType.NVarChar).Value =
                (object?)ToPipeSeparated(request.MonthClosure) ?? DBNull.Value;

            cmd.Parameters.Add("@StartDateFrom", SqlDbType.DateTime).Value =
                request.StartDateFrom ?? (object)DBNull.Value;

            cmd.Parameters.Add("@StartDateTo", SqlDbType.DateTime).Value =
                request.StartDateTo ?? (object)DBNull.Value;

            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = request.PageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = request.PageSize;

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (totalCount == 0)
                {
                    totalCount = reader["TotalCount"] != DBNull.Value
                        ? Convert.ToInt32(reader["TotalCount"])
                        : 0;
                }

                tickets.Add(new TicketFilteredData
                {
                    DetailedStatus = reader["DetailedStatus"]?.ToString(),
                    RequestID = Convert.ToInt32(reader.GetOrdinal("RequestID")),
                    Client = reader["Client"]?.ToString(),
                    MonthClosure = reader["MonthClosure"]?.ToString(),
                    TicketStatus = reader["TicketStatus"]?.ToString(),
                    Comments = reader["Comments"]?.ToString()
                });
            }

            return (tickets, totalCount);
        }
        #endregion

        #region Dropdown

        public async Task<TicketDropdownData> GetTicketDropdownDataAsync(DataConnection dbConnection)
        {
            var result = new TicketDropdownData();

            using var conn = new SqlConnection(dbConnection.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = CreateCommand(conn, "dbo.sp_GetTicketDropdownData");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.DetailedStatus.Add(reader["DetailedStatus"].ToString());
                result.MonthClosure.Add(reader["MonthClosure"].ToString());
                result.TicketStatus.Add(reader["TicketStatus"].ToString());
            }

            return result;
        }

        #endregion

        #region FilteredData
        public async Task<SummaryResponseNew> GetSummaryFilteredDataAsync(
            DataConnection dbConnection,
            List<string> groupNamesList,
            List<string> clientsList,
            List<string> statusesList,
            DateTime? startDateFrom,
            DateTime? startDateTo)
        {
            var response = new SummaryResponseNew();

            using var conn = new SqlConnection(dbConnection.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("[dbo].[sp_GetSummaryFilteredData]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;

            string? statuses = statusesList?.Any() == true ? string.Join('|', statusesList) : null;
            string? groups = groupNamesList?.Any() == true ? string.Join('|', groupNamesList) : null;
            string? clients = clientsList?.Any() == true ? string.Join('|', clientsList) : null;

            cmd.Parameters.Add("@StartDateFrom", SqlDbType.DateTime).Value =
                startDateFrom ?? (object)DBNull.Value;

            cmd.Parameters.Add("@StartDateTo", SqlDbType.DateTime).Value =
                startDateTo ?? (object)DBNull.Value;

            cmd.Parameters.Add("@Statuses", SqlDbType.NVarChar).Value =
                (object?)statuses ?? DBNull.Value;

            cmd.Parameters.Add("@Groups", SqlDbType.NVarChar).Value =
                (object?)groups ?? DBNull.Value;

            cmd.Parameters.Add("@Clients", SqlDbType.NVarChar).Value =
                (object?)clients ?? DBNull.Value;

            cmd.Parameters.Add("@StatusesCount", SqlDbType.Int).Value =
                statusesList?.Count ?? 0;

            cmd.Parameters.Add("@GroupsCount", SqlDbType.Int).Value =
                groupNamesList?.Count ?? 0;

            cmd.Parameters.Add("@ClientsCount", SqlDbType.Int).Value =
                clientsList?.Count ?? 0;

            var rawData = new List<(string TicketStatus, string Client, string MonthClosure)>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rawData.Add((
                    TicketStatus: reader["TicketStatus"]?.ToString(),
                    Client: reader["Client"]?.ToString(),
                    MonthClosure: reader["MonthClosure"]?.ToString()
                ));
            }

            // Pivot in C#
            var grouped = rawData
                .GroupBy(r => new { r.TicketStatus, r.Client })
                .Select(g =>
                {
                    var monthCounts = g.GroupBy(x => x.MonthClosure)
                                       .ToDictionary(x => x.Key, x => x.Count());

                    int grandTotal = monthCounts.Values.Sum();
                    monthCounts["Grand total"] = grandTotal;

                    return new SummaryData
                    {
                        TicketStatus = g.Key.TicketStatus,
                        Client = g.Key.Client,
                        MonthCounts = monthCounts
                    };
                })
                .OrderByDescending(x => x.TicketStatus)
                .ThenBy(x => x.Client)
                .ToList();

            response.SummaryData.AddRange(grouped);

            return response;
        }
        #endregion

        #region DetailedStatusByAIGroup
        public async Task<SummaryResponse> GetClientAndDetailedStatusByAIGroupAsync(
            DataConnection dbConnection,
            List<string> groupNames,
            DateTime? startDate,
            DateTime? endDate)
        {
            var response = new SummaryResponse();

            if (groupNames == null || !groupNames.Any())
                return response;

            using var conn = new SqlConnection(dbConnection.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = CreateCommand(conn, "dbo.sp_GetClientAndDetailedStatusByAIGroup");

            cmd.Parameters.AddWithValue("@GroupNames", ToPipeSeparated(groupNames));
            cmd.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.DetailedStatuses.Add(new StatusCount
                {
                    StatusName = reader["DetailedStatus"].ToString(),
                    StatusNameCount = Convert.ToInt32(reader["StatusCount"])
                });
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    response.Clients.Add(new ClientCount
                    {
                        Client = reader["Client"].ToString(),
                        ClientCountValue = Convert.ToInt32(reader["ClientCount"])
                    });
                }
            }

            return response;
        }
        #endregion

        #region DetailedStatusByAIGroupAndClient
        public async Task<SummaryResponse> GetDetailedStatusByAIGroupAndClientAsync(
            DataConnection dbConnection,
            List<string> groupNames,
            List<string> clients,
            DateTime? startDateFrom,
            DateTime? startDateTo)
        {
            var response = new SummaryResponse();

            using var conn = new SqlConnection(dbConnection.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = CreateCommand(conn, "dbo.sp_GetDetailedStatusByAIGroupAndClient");

            cmd.Parameters.AddWithValue("@GroupNames", (object?)ToPipeSeparated(groupNames) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Clients", (object?)ToPipeSeparated(clients) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDateFrom", startDateFrom ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDateTo", startDateTo ?? (object)DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.DetailedStatuses.Add(new StatusCount
                {
                    StatusName = reader["DetailedStatus"].ToString(),
                    StatusNameCount = Convert.ToInt32(reader["StatusCount"])
                });
            }
            return response;
        }
        #endregion

        #region Helpers

        private static SqlCommand CreateCommand(SqlConnection conn, string spName)
        {
            return new SqlCommand(spName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = DefaultTimeout
            };
        }

        private static void AddDateParameters(SqlCommand cmd, DateTime? start, DateTime? end)
        {
            cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = (object?)start ?? DBNull.Value;
            cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = (object?)end ?? DBNull.Value;
        }

        private static string? ToPipeSeparated(List<string>? values)
        {
            if (values == null || !values.Any())
                return null;

            return string.Join("|", values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()));
        }
        #endregion
    }
}
