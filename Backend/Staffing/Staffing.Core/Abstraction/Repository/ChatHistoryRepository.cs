using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using Staffing.Core.Abstraction.Infrastructure;
using System.Data;
using System.Security.Claims;


namespace Staffing.Core.Abstraction.Repository
{
    public class ChatHistoryRepository : IChatHistoryRepository
    {
        private readonly string _connectionString;
        public AuthenticatedUser AuthUser { get; set; } = new();
        public ChatHistoryRepository(IConfiguration config, IHttpContextAccessor contextAccessor)
        {
            _connectionString = config.GetConnectionString("StaffingDbConnection")
            ?? config.GetValue<string>("StructuredSearch:StaffingDbConnection:ConnectionString");


            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'StaffingDbConnection' is not configured. " +
                    "Please add it to 'ConnectionStrings:StaffingDbConnection' or 'StructuredSearch:StaffingDbConnection:ConnectionString'.");
            }
            ClaimsPrincipal user = contextAccessor.HttpContext?.User;
            ClaimsIdentity identity = (ClaimsIdentity)user.Identity;
            if (identity?.IsAuthenticated == true)
            {
                var claim = user.FindFirst("claimjson")?.Value;
                if (!string.IsNullOrWhiteSpace(claim))
                {
                    try
                    {
                        AuthUser = JsonConvert.DeserializeObject<AuthenticatedUser>(claim) ?? new();
                    }
                    catch
                    {
                        AuthUser = new();
                    }
                }
            }


        }
        // Helper: returns current authenticated user id or null if not authenticated

        /// <summary>
        /// Save conversation as a single JSON blob row per conversationId + user.
        /// System messages are excluded from the stored JSON.
        /// </summary>
        public async Task SaveAsync(string conversationId, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(conversationId)) return;

            // filter out system messages
            var filtered = messages
                .Where(m => !string.Equals(m.Role?.Trim(), "system", StringComparison.OrdinalIgnoreCase))
                .Select(m => new ChatMessage
                {
                    Role = m.Role,
                    Content = m.Content,
                    Message = m.Message,
                    Timestamp = m.Timestamp == default ? DateTime.UtcNow : m.Timestamp
                })
                .ToList();

            // determine user id (nullable)
            int? userId = AuthUser?.Id > 0 ? AuthUser.Id : (int?)null;
            string? userIdValue = userId.HasValue ? userId.Value.ToString() : null;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var tran = conn.BeginTransaction();
            try
            {
                // delete existing aggregated row(s) for this session + user
                using (var delCmd = new SqlCommand(@"
                DELETE FROM dbo.UserChatMessageHistory
                WHERE ConversationId = @conversationId
                  AND ((@userId IS NULL AND UserId IS NULL) OR (UserId = @userId))
                ", conn, tran))
                {
                    delCmd.Parameters.Add(new SqlParameter("@conversationId", SqlDbType.NVarChar, 200) { Value = conversationId });
                    delCmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.NVarChar, 200) { Value = (object?)userIdValue ?? DBNull.Value });
                    await delCmd.ExecuteNonQueryAsync(cancellationToken);
                }

                // If nothing to store after filtering, we're done (existing rows removed)
                if (!filtered.Any())
                {
                    tran.Commit();
                    return;
                }

                // insert a single row containing JSON array of messages
                var contentJson = JsonConvert.SerializeObject(filtered, Formatting.None);

                using var insertCmd = new SqlCommand(@"INSERT INTO dbo.UserChatMessageHistory (ConversationId, UserId, Role, Content, Timestamp)
                                                      VALUES (@conversationId, @userId, @role, @content, @timestamp)", conn, tran);

                insertCmd.Parameters.Add(new SqlParameter("@conversationId", SqlDbType.NVarChar, 200) { Value = conversationId });
                insertCmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.NVarChar, 200) { Value = (object?)userIdValue ?? DBNull.Value });
                // Role column left NULL for aggregated row (or you can set 'conversation')
                insertCmd.Parameters.Add(new SqlParameter("@role", SqlDbType.NVarChar, 50) { Value = DBNull.Value });
                insertCmd.Parameters.Add(new SqlParameter("@content", SqlDbType.NVarChar, -1) { Value = (object?)contentJson ?? DBNull.Value });
                insertCmd.Parameters.Add(new SqlParameter("@timestamp", SqlDbType.DateTime2) { Value = DateTime.UtcNow });

                await insertCmd.ExecuteNonQueryAsync(cancellationToken);

                tran.Commit();
            }
            catch
            {
                try { tran.Rollback(); } catch { }
                throw;
            }
        }

        /// <summary>
        /// Delete aggregated row(s) for the conversation and current user.
        /// </summary>
        public async Task DeleteBySessionAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(conversationId)) return;

            int? userId = AuthUser?.Id > 0 ? AuthUser.Id : (int?)null;
            string? userIdValue = userId.HasValue ? userId.Value.ToString() : null;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = new SqlCommand(@"
            DELETE FROM dbo.UserChatMessageHistory
            WHERE ConversationId = @conversationId
              AND ((@userId IS NULL AND UserId IS NULL) OR (UserId = @userId))
            ", conn);
            cmd.Parameters.Add(new SqlParameter("@conversationId", SqlDbType.NVarChar, 200) { Value = conversationId });
            cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.NVarChar, 200) { Value = (object?)userIdValue ?? DBNull.Value });
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Load aggregated JSON row(s) and reconstruct ChatHistory.
        /// Handles both legacy per-message rows and new aggregated JSON rows.
        /// Skips system messages.
        /// </summary>
        public async Task<ChatHistory?> LoadAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(conversationId)) return null;

            int? userId = AuthUser?.Id > 0 ? AuthUser.Id : (int?)null;
            string? userIdValue = userId.HasValue ? userId.Value.ToString() : null;

            var rows = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = new SqlCommand(@"
            SELECT Role, Content
            FROM dbo.UserChatMessageHistory
            WHERE ConversationId = @conversationId
              AND ((@userId IS NULL AND UserId IS NULL) OR (UserId = @userId))
            ORDER BY Timestamp", conn);
            cmd.Parameters.Add(new SqlParameter("@conversationId", SqlDbType.NVarChar, 200) { Value = conversationId });
            cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.NVarChar, 200) { Value = (object?)userIdValue ?? DBNull.Value });

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var content = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                rows.Add(content);
            }

            if (!rows.Any()) return null;

            var history = new ChatHistory();

            foreach (var content in rows)
            {
                if (string.IsNullOrWhiteSpace(content)) continue;

                var trimmed = content.TrimStart();
                // aggregated JSON (array of messages)
                if (trimmed.StartsWith("[") || trimmed.StartsWith("{"))
                {
                    try
                    {
                        // Try deserialize to list of ChatMessage (new format)
                        var list = JsonConvert.DeserializeObject<List<ChatMessage>>(content);
                        if (list != null)
                        {
                            foreach (var m in list)
                            {
                                if (string.Equals(m.Role, "system", StringComparison.OrdinalIgnoreCase))
                                    continue; // skip system messages
                                if (string.Equals(m.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                                    history.AddSystemMessage(m.Content);
                                else
                                    history.AddUserMessage(m.Content);
                            }
                            continue;
                        }
                    }
                    catch
                    {
                        // fall back to treating content as plain text
                    }
                }

                // legacy single-message rows: treat as user message by default, except when Role indicates assistant/system
                // Attempt to read role from the DB row if needed — we didn't fetch Role earlier; re-query would be heavier.
                // For now treat legacy rows as user messages (safe fallback)
                history.AddUserMessage(content);
            }

            return history;
        }
    }
}