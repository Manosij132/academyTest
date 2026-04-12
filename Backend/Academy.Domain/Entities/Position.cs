
namespace Academy.Domain.Entities
{
    public class Position 
    {
        public decimal Id { get; set; }
        public decimal PositionId { get; set; }
        public decimal? SrNumber { get; set; }
        public string Client { get; set; }
        public string ProjectName { get; set; }
        public string BusinessUnit { get; set; }
        public string Region { get; set; }
        public string PositionStudio { get; set; }
        public string PositionTitle { get; set; }
        public string srPosition { get; set; }
        public string Seniority { get; set; }
        public string TypeOfPosition { get; set; }
        public string AIPodRole { get; set; }
        public string IndustrySpecialization { get; set; }
        public bool? ContractorAllowed { get; set; }
        public string WorkOffice { get; set; }
        public string SecondaryLocation { get; set; }
        public string PositionFramework { get; set; }
        public bool? ClientInterviewRequired { get; set; }
        public bool? EnglishRequired { get; set; }
        public decimal? PositionLoad { get; set; }
        public DateTime? StartDate { get; set; }
        public string GloberToBeAssigned { get; set; }
        public string Stage { get; set; }
        public bool? Replacement { get; set; }
        public string Handler { get; set; }
        public string AssociateHandler { get; set; }
        public string HandlerTeam { get; set; }
        public string AssociateHandlerTeam { get; set; }
        public string RateAmount { get; set; }
        public string RatePeriod { get; set; }
        public string Submitter { get; set; }
        public DateTime? SubmitDate { get; set; }
        public int? Aging { get; set; }
        public DateTime? EstimatedStaffingDate { get; set; }
        public DateTime? LastSyncedUtc { get; set; }
        public bool? IsActive { get; set; }
    }
}
