using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.ChatCompletion; // adjust if namespace differs

public static class ChatHistoryExtensions
{
    // Normalizes whitespace and lowercases for safer duplicate detection
    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var s = Regex.Replace(text, @"\s+", " ").Trim();
        return s.ToLowerInvariant();
    }

    // Adds a system message only if an equivalent one doesn't already exist
    public static void AddDistinctSystemMessage(this ChatHistory history, string content)
    {
        if (history == null) throw new ArgumentNullException(nameof(history));
        if (content == null) content = string.Empty;

        string normNew = Normalize(content);

        // Try common property names - some SK versions use .Messages, others different accessors
        var messages = GetMessagesEnumerable(history);

        bool exists = messages.Any(m =>
        {
            var role = GetRole(m);
            var text = GetContent(m);
            return (role == "system" || role == "user") && Normalize(text) == normNew;
        });

        if (!exists)
        {
            history.AddSystemMessage(content); // typical SK helper
        }
    }

    // Helpers that attempt to read common ChatMessage properties using duck-typing
    private static IEnumerable<object> GetMessagesEnumerable(ChatHistory history)
    {
        // prefers a Messages property; fallback to enumeration if ChatHistory itself is enumerable
        var t = history.GetType();
        var messagesProp = t.GetProperty("Messages");
        if (messagesProp != null)
        {
            var val = messagesProp.GetValue(history) as System.Collections.IEnumerable;
            if (val != null) return val.Cast<object>();
        }

        // if ChatHistory itself implements IEnumerable<ChatMessage>
        if (history is System.Collections.IEnumerable enumerable)
        {
            return enumerable.Cast<object>();
        }

        // else return empty
        return Enumerable.Empty<object>();
    }

    private static string GetRole(object msg)
    {
        if (msg == null) return string.Empty;
        var t = msg.GetType();
        var p = t.GetProperty("Role") ?? t.GetProperty("Author") ?? t.GetProperty("From");
        if (p != null)
        {
            var v = p.GetValue(msg);
            return v?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }

    private static string GetContent(object msg)
    {
        if (msg == null) return string.Empty;
        var t = msg.GetType();
        var p = t.GetProperty("Content") ?? t.GetProperty("Text") ?? t.GetProperty("Message");
        if (p != null)
        {
            var v = p.GetValue(msg);
            return v?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }
}
