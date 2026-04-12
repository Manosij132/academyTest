namespace Academy.Shared.Constants
{
    public class DbConstants
    {
        #region SP Names
        public const string FETCH_DASHBOARD_TRAININGS = "usp_FetchDashboardTrainings";
        public const string FETCH_PROFICIENCIES = "usp_FetchProficiencies";
        public const string FETCH_SKILL_ENDORSEMENT = "usp_FetchSkillEndorsement";
        public const string FETCH_SKILL_TRAININGS_METADATA = "usp_FetchSkillTrainingsMetaData";
        public const string FETCH_EMPLOYEES_STARTS_WITH = "usp_FetchEmployeesStartsWith";
        public const string FETCH_GEXLEADER_STARTS_WITH = "usp_FetchDojoGexLeaderStartsWith";
        public const string FETCH_DOJO_REPORT_FILTERS = "usp_GetFiltersForDojoActivityReport";
        public const string CREATE_ECOSYSTEM_IF_NOT_EXISTS = "usp_CreateEcosystemIfNotExists";
        public const string MAP_SENIORITY = "usp_MapSeniority";
        public const string FETCH_LATEST_COMMENT = "usp_FetchLatestComment";
        public const string EXECUTE_TRAINING_ASSIGNMENT = "usp_ProcessTrainingAssignment";
        public const string EXECUTE_AUTO_TRAINING_ASSIGNMENT = "usp_ProcessAutoTrainingAssignment";
        public const string EXECUTE_REMINDERS = "usp_SetReminders";
        public const string FETCH_AI_SKILL_TRAININGS_METADATA = "usp_FetchAISkillTrainingsMetaData";
        #endregion

        #region Parameter Names
        public const string PARAM_EMPLOYEE_ID = "@employeeId";
        public const string PARAM_ECOSYSTEM_ID = "@ecosystemId";
        public const string PARAM_CLIENT = "@client";
        public const string PARAM_TRANSACTION_ID = "@transactionId";
        public const string PARAM_WHERE = "@where";
        public const string PARAM_FORCE = "@force";
        public const string PARAM_REMINDER_EMAIL_SUBJECT = "@reminderMailSubject";
        public const string PARAM_REMINDER_EMAIL_TEMPLATE = "@reminderTemplateName";
        public const string PARAM_BCC = "@bcc";
        public const string PARAM_TRAINING_ASSIGNMENT_SRC = "@trainingAssignmentSrc";
        #endregion
    }
}
