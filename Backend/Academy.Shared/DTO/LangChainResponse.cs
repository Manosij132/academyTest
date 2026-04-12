using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.DTO
{
    public class LangChainResponse
    {
        public string Message { get; set; }
        public string Type { get; set; } = "Message";
        public bool Status { get; set; }
        public object Data { get; set; }
    }
    public class TrainingInput
    {
        public int EcoSystemId { get; set; }
        public string EcoSystem { get; set; }
        public string[] EmployeeEmail { get; set; }
        public int[] TrainingIds { get; set; }
        public string Account { get; set; }
        public string ResourceType { get; set; }
        public string TrainingName { get; set; }
        public string ForAllEmployees { get; set; }
    }
    public class ChatMessage
    {
        public string Role { get; set; }            // "user", "assistant", etc.
        public string Message { get; set; }         // Text content
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }     // When the message was added
    }
    public class ToolLogEntry
    {
        public TrainingInput Input { get; set; }
        public string Output { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class SessionMemory
    {
        // Chat memory like ConversationBufferMemory
        public List<ChatMessage> ChatHistory { get; set; } = new();
        // Tool logs: Tool name => List of tool interactions
        public Dictionary<string, List<ToolLogEntry>> ToolLogs { get; set; } = new();
    }
}
