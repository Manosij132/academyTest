using Microsoft.Data.SqlClient;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using System.Data;

namespace Staffing.Core.Abstraction.Services
{
    public class StaffRequestService : IStaffRequestService
    {
        private static SqlCommand CreateCommand(SqlConnection conn, string spName)
        {
            var cmd = new SqlCommand(spName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            return cmd;
        }

        public async Task<PagedResult<StaffRequestDto>> QueryStaffRequestsByDateAsync(
            DataConnection dbConn,
            string? startDate,
            string? endDate,
            string? searchText,
            int pageNumber,
            int pageSize)
        {
            var result = new PagedResult<StaffRequestDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            using var conn = new SqlConnection(dbConn.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("dbo.sp_StaffRequest_Search", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // Strongly typed parameters (production safe)
            cmd.Parameters.Add("@StartDate", SqlDbType.DateTime)
                .Value = string.IsNullOrWhiteSpace(startDate)
                    ? DBNull.Value
                    : Convert.ToDateTime(startDate);

            cmd.Parameters.Add("@EndDate", SqlDbType.DateTime)
                .Value = string.IsNullOrWhiteSpace(endDate)
                    ? DBNull.Value
                    : Convert.ToDateTime(endDate);

            cmd.Parameters.Add("@SearchText", SqlDbType.NVarChar, 200)
                .Value = string.IsNullOrWhiteSpace(searchText)
                    ? DBNull.Value
                    : searchText;

            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            using var reader = await cmd.ExecuteReaderAsync();

            // First result → total count
            if (await reader.ReadAsync())
                result.TotalRecords = reader.GetInt32(0);

            // Second result → paged data
            await reader.NextResultAsync();

            while (await reader.ReadAsync())
                result.Data.Add(MapReaderToDto(reader));

            return result;
        }
        public async Task<StaffRequestDto?> GetStaffRequestByIdAsync(
            DataConnection dbConn,
            int id)
        {
            using var conn = new SqlConnection(dbConn.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = CreateCommand(conn, "dbo.sp_StaffRequest_GetById");
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapReaderToDto(reader);
        }

        public async Task<int> UpdateStaffRequestEditableFieldsAsync(
            DataConnection dbConn,
            int id,
            StaffRequestUpdateDto dto)
        {
            using var conn = new SqlConnection(dbConn.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = CreateCommand(conn, "dbo.sp_StaffRequest_UpdateEditable");

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@DetailedStatus", (object?)dto.DetailedStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MonthClosure", (object?)dto.MonthClosure ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TicketStatus", (object?)dto.TicketStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Comments", (object?)dto.Comments ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync();
        }

        private static StaffRequestDto MapReaderToDto(SqlDataReader reader)
        {
            return new StaffRequestDto
            {
                RequestID = Convert.ToInt32(reader["RequestID"]),
                Client = reader["Client"]?.ToString() ?? "",
                Project = reader["Project"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                PositionID = reader["PositionID"]?.ToString() ?? "",
                Seniority = reader["Seniority"]?.ToString() ?? "",
                Stage = reader["Stage"]?.ToString() ?? "",
                StartDate = reader["StartDate"] as DateTime?,
                SubmitDate = reader["SubmitDate"] as DateTime?,
                Handler = reader["Handler"]?.ToString() ?? "",
                DetailedStatus = reader["DetailedStatus"]?.ToString() ?? "",
                MonthClosure = reader["MonthClosure"]?.ToString() ?? "",
                TicketStatus = reader["TicketStatus"]?.ToString() ?? "",
                Comments = reader["Comments"]?.ToString() ?? ""
            };
        }
    }
}
