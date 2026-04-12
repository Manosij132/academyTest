namespace Academy.Shared.Constants
{
    public class ApplicationConstants
    {
        public const string DAILY_REMINDER = "DAILY_REMINDER";
        public const string HTML_PLACEHOLDER_APP_URI = "{{AppUri}}";
        public const string HTML_PLACEHOLDER_TRAININGS = "{{Trainings}}";
        public const string HTML_PLACEHOLDER_GLOBER = "{{Glober}}";
        public const string HTML_PLACEHOLDER_YEAR = "{{Year}}";

        public static readonly List<short?> ALLOWED_SENIORITIES = [3, 4, 5, 6, 7, 8, 9];

        public static readonly Dictionary<short?, string> ALLOWED_SENIORITIES_DETAILS = new()
        {
            { 3, "Architect (Sr Level 3)" },
            { 4, "Software Designer (Sr Level 2)" },
            { 5, "Sr (Sr Level 1)" },
            { 6, "SSr Adv" },
            { 7, "SSr" },
            { 8, "Jr Adv" },
            { 9, "Jr" }
        };

        public const string MIME_TYPE_GOOGLE_DRIVE_FOLDER = "application/vnd.google-apps.folder";
        public const string MIME_TYPE_JSON = "application/json";
    }
}
