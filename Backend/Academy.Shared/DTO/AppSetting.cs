using Google.Apis.Auth.OAuth2;

namespace Academy.Shared.DTO
{
    public class AppSetting
    {
        public string DateTimeAsIdFormat { get; set; } = string.Empty;
        public string ExportReportDriveId { get; set; } = string.Empty;
        public string SpinTrainingRequestDriveId { get; set; } = string.Empty;
        public string DojoEngagementReportFolderId { get; set; } = string.Empty;
        public string IssuerWebAuthority { get; set; } = string.Empty;
        public string LoggedInUserEmail { get; set; } = string.Empty;
        public JWTSettings JWTSetting { get; set; } = new();
        public bool AuthenticateLocal { get; set; } = false;
        public int SystemUser { get; set; }
        public string AppUri { get; set; }
        public List<EmailSetting> EmailSettings { get; set; } = new();
        public string DailyRemindersBcc { get; set; }
        public string ReportsCc { get; set; }
        public string SlackBotToken { get; set; }
        public string SlackChannelId { get; set; }
        public bool SendEmailEnabled { get; set; }
        public GlobantSmtpSettings GlobantSmtpSetting { get; set; } = new();
        public List<ExportReport> ExportReports { get; set; } = new();
        public WorkerConfig EmailWorkerConfig { get; set; }
        public WorkerConfig SyncEmployeeWorkerConfig { get; set; }
        public WorkerConfig TrainingAssignmentWorkerConfig { get; set; }
        public WorkerConfig ReminderWorkerConfig { get; set; }
        public WorkerConfig ExportReportWorkerConfig { get; set; }
        public SheetDirectory SheetDirectory { get; set; } = new();
        public GoogleJsonCredentials CredentialsJson { get; set; }
        public List<string> SyncColumns { get; set; } = new();
        public OpenAISettings OpenAISettings { get; set; }
        public string CvProfileFolderId { get; set; }
        public GoogleCalender GoogleCalender { get; set; } = new();
        public int NoOfRequestPerUser { get; set; }
    }

    public class SheetDirectory 
    {
        public string SyncDataWorksheetId { get; set; }
        public string SyncDataSheetName { get; set; }
        public string SyncDataRange { get; set; }
    }

    public class ExportReport
    {
        public string Key { get; set; }
        public string SpreadsheetId { get; set; }
        public string SheetName { get; set; }
        public int SheetId { get; set; }
        public string Range { get; set; }
        public string Command { get; set; }
    }

    public class GlobantSmtpSettings
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public bool RequireSsl { get; set; }
        public string SenderEmail { get; set; }
        public string SenderKey { get; set; }
        public string SenderDisplayName { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }

    public class EmailSetting
    {
        public string Key { get; set; }
        public string Subject { get; set; }
        public string File { get; set; }
    }

    public class JWTSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double DurationInMinutes { get; set; } = 0;
    }

    public class KeyValueSetting<T1, T2>
    {
        public T1 Key;
        public T2 Value;
    }

    public class WorkerConfig
    {
        public bool Enabled { get; set; }
        public string Schedule { get; set; }
    }

    public class GoogleJsonCredentials
    {
        public string Type { get; set; }
        public string ProjectId { get; set; }
        public string PrivateKeyId { get; set; }
        public string PrivateKey { get; set; }
        public string ClientEmail { get; set; }
        public string ClientId { get; set; }
        public string TokenUri { get; set; }
    }

    public class OpenAISettings
    {
        public string ModelId { get; set; }
        public string ApiKey { get; set; }
        public string EndPoint { get; set; }
        public string Rag_AI_Token { get; set; }
        
    }

    public class GoogleCalender
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string KeyEnDecrypt { get; set; }
    }
    
}
