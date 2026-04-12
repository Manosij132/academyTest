namespace Staffing.Shared.Logging
{
    public class FileLoggerOptions
    {
        public virtual string FilePath { get; set; } = default!;

        public virtual string FolderPath { get; set; } = default!;
    }
}
