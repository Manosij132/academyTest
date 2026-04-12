namespace Academy.Core.Models
{
    public interface ISmtpSettings
    {
        string Host { get; }
        int Port { get; }
        string UserName { get; }
        string Password { get; }
        bool UseDefaultCredentials { get; }
        bool EnableSsl { get; }
        string SenderEmail { get; }
        string SenderName { get; }
    }
}
