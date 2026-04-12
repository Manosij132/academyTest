using Academy.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Core.Shared
{
    public class MemoryStore
    {
        private readonly Dictionary<string, SessionMemory> _sessions = new();
        private static readonly Lazy<MemoryStore> _instance = new(() => new MemoryStore());

        public static MemoryStore Instance => _instance.Value;

        private MemoryStore() { }

        public SessionMemory GetOrCreateSession(string sessionId)
        {
            if (!_sessions.ContainsKey(sessionId))
                _sessions[sessionId] = new SessionMemory();
            return _sessions[sessionId];
        }

        public void AddChat(string sessionId, string role, string message)
        {
            var session = GetOrCreateSession(sessionId);
            session.ChatHistory.Add(new ChatMessage() { Role = role, Message = message, Timestamp = DateTime.Now });
        }

        public void LogToolUsage(string sessionId, string toolName, TrainingInput input, string output)
        {
            var session = GetOrCreateSession(sessionId);
            if (!session.ToolLogs.ContainsKey(toolName))
                session.ToolLogs[toolName] = new List<ToolLogEntry>();

            session.ToolLogs[toolName].Add(new ToolLogEntry { Input = input, Output = output, Timestamp = DateTime.Now });
        }

        // Optionally: Cleanup only old messages inside active sessions
        public void CleanupOldMessages(int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);

            foreach (var session in _sessions.Values)
            {
                foreach(var toolLog in session.ToolLogs.Values)
                {
                    toolLog.RemoveAll(m => m.Timestamp < cutoff);
                }
            }
        }

        public List<ChatMessage> GetChatHistory(string sessionId)
        {
            return GetOrCreateSession(sessionId).ChatHistory;
        }

        public Dictionary<string, List<ToolLogEntry>> GetSessionLogs(string sessionId)
        {
            return GetOrCreateSession(sessionId).ToolLogs;
        }

        public List<ToolLogEntry> GetToolLogs(string sessionId, string toolName)
        {
            return GetOrCreateSession(sessionId)?.ToolLogs
                    .Where(t => t.Key.ToLower() == toolName.ToLower())
                        .SelectMany(t => t.Value).ToList();
        }

        public ToolLogEntry GetMissingFieldFromHistory(string sessionId, string fieldName)
        {
            var toolLogEntries = GetOrCreateSession(sessionId)?.ToolLogs.
                SelectMany(kvp => kvp.Value)
                    .OrderBy(o => o.Timestamp)
                        .LastOrDefault(s => {
                            var prop = typeof(TrainingInput).GetProperty(fieldName);
                            if (prop == null) return false;

                            var value = prop.GetValue(s.Input);

                            // Null or default check
                            if (value == null) return false;

                            var defaultValue = value.GetType().IsValueType
                                ? Activator.CreateInstance(value.GetType())
                                : null;

                            return !value.Equals(defaultValue);
                        });
            return toolLogEntries;
        }
    }

}
