namespace Academy.Shared.Constants
{
    public class Messages
    {
        public const string ERROR_UnauthorizedAccess = "You are not authorized to access this resource";
        public const string ERROR_InSufficientPermissions = "You are not authorized to perform this action";
        public const string ERROR_EmployeeNotFoundOrInaccessible = "No employee exists for given id or you do not have required rights to view the employee details.";
        public const string ERROR_InvalidStatusChangeRequest = "Invalid operation: The requested status change is not permitted.";
        public const string ERROR_EmployeeIsNull = "The Employee record is null for {0}";
        public const string ERROR_EndDateIsLessThanStartDate = "Provided ExpectedEndDate is smaller than Start Date";
        public const string ERROR_EcosystemNotFound = "Working Ecosystem Not Exists";
        public const string ERROR_SeniorityNotFound = "Seniority Not Exists";
        public const string ERROR_Generic = "An error occurred while saving the data. Please try again later";
        public const string ERROR_BadRequest = "BadRequest: Supplied Parameters are not valid";
        public const string SUCCESS_GENERIC = "The data has been saved successfully";
        public const string ERROR_CONFIG_KEY_NOT_FOUND = "{0} key is missing or has no value configured.";
        public const string ERROR_BookMarkTemplateNotFound = "BookMark Template Not Exists";
        public const string ERROR_ExportReportNotConfigured = "Export for this report is not configured.";
        public const string ERROR_AIAgentLogErrorPrefix = "AI agent failed to answer for question:";
        public const string ERROR_AIAgentLogErrorMessage = "The AI agents are having trouble communicating. This may cause delays in processing your request. Please bear with us while this is resolved. Would you like to try something else?";

    }
}
