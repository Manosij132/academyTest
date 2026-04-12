using Academy.Domain.Entities;

namespace Academy.Workers.SyncEmployeeWorker
{
    internal class EmployeeSheetColumnIndex
    {
       internal int indexOfEmployeeName { get; set; }
       internal int indexOfEmployeeEmail { get; set; }
       internal int indexOfLeaderEmail { get; set; }
       internal int indexOfSeniority { get; set; }
       internal int indexOfPosition { get; set; }
       internal int indexOfDesignation { get; set; }
       internal int indexOfClient { get; set; }
       internal int indexOfProject { get; set; }
       internal int indexOfTdc { get; set; }
       internal int indexOfBaseLocation { get; set; }
       internal int indexOfCommunity { get; set; }
       internal int indexOfJoiningDate { get; set; }
       internal int indexOfStatus { get; set; }
       internal int indexOfStudio { get; set; }
       internal int indexOfGender { get; set; }
       internal int indexOfWorkingEcosystem { get; set; }
       internal int indexOfGexLeaders { get; set; }
       internal int indexOfGloberId { get; set; }
       internal int indexOfResignationDate { get; set; }
       internal int indexOfLastDate { get; set; }
       internal int indexOfGlobantTenure { get; set; }
       internal int indexOfTotalExperience { get; set; }
       internal int indexOfInTP { get; set; }
       internal int indexOfExp { get; set; }

        public EmployeeSheetColumnIndex(IList<object> headers)
        {
            indexOfEmployeeName = headers.IndexOf("Employee Name");
            indexOfEmployeeEmail = headers.IndexOf("Globant email ids");
            indexOfLeaderEmail = headers.IndexOf("Career Leader");
            indexOfGender = headers.IndexOf("Gender");
            indexOfSeniority = headers.IndexOf("Seniority");
            indexOfPosition = headers.IndexOf("Position");
            indexOfDesignation = headers.IndexOf("Position");
            indexOfClient = headers.IndexOf("Client");
            indexOfProject = headers.IndexOf("Project");
            indexOfTdc = headers.IndexOf("TDC");
            indexOfBaseLocation = headers.IndexOf("Hiring Location");
            indexOfCommunity = headers.IndexOf("Community");
            indexOfJoiningDate = headers.IndexOf("Joining date");
            indexOfStatus = headers.IndexOf("Status");
            indexOfStudio = headers.IndexOf("Studio");
            indexOfWorkingEcosystem = headers.IndexOf("Working Ecosystem");
            indexOfGexLeaders = headers.IndexOf("Account Leader(s)");
            indexOfGloberId = headers.IndexOf("Global ID");
            indexOfResignationDate = headers.IndexOf("Resignation Date");
            indexOfLastDate = headers.IndexOf("Last Date");
            indexOfGlobantTenure = headers.IndexOf("Globant Tenure");
            indexOfTotalExperience = headers.IndexOf("Total experience");
            indexOfTotalExperience = headers.IndexOf("In TP?");
            indexOfTotalExperience = headers.IndexOf("Exp");
        }
    }
}
