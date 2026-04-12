namespace Academy.Shared.DTO
{
    public class AIEvaluationDataModel
    {
        public string AIEvaluationStatus { get; set; }
        public string AIEvaluationStatusComment { get; set; }
        public string Strengths { get; set; }
        public string AreasForImprovement { get; set; }
        public string OverallEvaluation { get; set; }
        public decimal FinalRatingInNumber { get; set; }
        public string FinalRatingInText { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string PanelName { get; set; }
        public string PanelEmail { get; set; }
        public DateTime InterviewDate { get; set; }
        public string InterviewName { get; set; }
        public string? Interviewer { get; set; }
        public string? MainStream { get; set; }
        public string? StreamsCovered { get; set; }
        public int? TotalQuestionsAsked { get; set; }
        public string TargetExperienceTier { get; set; }
        public string? CandidateExperienceYears { get; set; }
        public string? QuestionsAskedByPanel { get; set; }
        public string? MaxRatingValue { get; set; }
    }
}
