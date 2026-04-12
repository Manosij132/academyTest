using Academy.Shared.Response;

namespace Academy.Shared.Exceptions
{
    public static class DomainErrors
    {
        public static class Common
        {
            public static Error NotFound(string id) => new("NotFound", $"The requested resource with id '{id}' was not found.");
            public static Error NullOrEmptyValue(string name) => new("NullValue", $"The value '{name}' cannot be null.");
            public static Error UnhandledException(string message) => new("UnhandledException", $"An unexpected error occurred.\n'{message}'");
        }
        public static class Employee
        {
            public static Error NotFound => new("EmployeeNotFound", "User not found.");
            public static Error EmployeeNotFoundOrInaccessible(string id) => new("EmployeeNotFoundOrInaccessible", $"No employee exists for given id '{id}' or you do not have required rights to view the employee details.");
            public static Error InvalidCredentials => new("InvalidCredentials", "Invalid credentials provided.");
            public static Error EmailAlreadyExists => new("EmailAlreadyExists", "An account with this email already exists.");
            public static Error GXLeaderAlreadyExists => new("GXLeaderAlreadyExists", "An employee with this GXLeader already exists.");
            public static Error MultipleGXLeaderExists => new("MultipleGXLeaderExists", "This employee has multiple GX Leaders assigned.");
            public static Error RemoveGXLeader => new("RemoveGXLeader", "GX Leader is not removed");
        }

        public static class Activity
        {
            public static Error ActivityMappingFailure => new("ActivityUpsertFailure", $"Activity updation failed.");
            public static Error ActivitiesCountZero=> new("ActivitiesCountZero", $"No activities for bulk assignment.");
            public static Error BulkInsertFailed => new("BulkInsertFailed", $"Bulk insert operation failed.");
        }

        public static class Authorization
        {
            public static Error UnauthorizedAccess => new("UnauthorizedAccess", "You do not have permission to access this resource.");
            public static Error AuthTokenExpired => new("AuthTokenExpired", "Authorization token expried!!");
            public static Error InvalidAuthToken => new("InvalidAuthToken", "Invalid authorization token!!");
        }

        public static class DashboardErrors
        {
            public static Error EndDateIsLessThanStartDate => new("EndDateIsLessThanStartDate", "Provided ExpectedEndDate is smaller than Start Date");
            public static Error InvalidStatusChangeRequest => new("InvalidStatusChangeRequest", "Invalid operation: The requested status change is not permitted.");
            public static Error InvalidStatusChangeRequestPendingToCompleted => new("InvalidStatusChangeRequest", "Invalid operation: The requested status change is not permitted from Pending to Completed.");
            public static Error InvalidTransactionId(string id) => new("NotFound", $"Invalid transaction id '{id}'.");
        }

    }
}
