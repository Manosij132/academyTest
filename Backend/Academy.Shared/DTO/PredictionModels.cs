using Microsoft.ML.Data;

namespace Academy.Shared.DTO
{
    public class InterviewData
    {

        [LoadColumn(0)]
        public float L1Select { get; set; }

        [LoadColumn(1)]
        public float L1Reject { get; set; }
        [LoadColumn(2)]
        public float GKSelect { get; set; }
        [LoadColumn(3)]
        public float GKReject { get; set; }
        [LoadColumn(4)]
        public float TotalCandidates { get; set; }
        [LoadColumn(5)]
        public float GKCandidatesPerSelection { get; set; }

        [LoadColumn(6)]
        public float L1CandidatesPerSelection { get; set; }

    }

    public class GKInterviewPrediction
    {
        [ColumnName("Score")]
        public float GKCandidatesPerSelection { get; set; }
    }

    public class L1InterviewPrediction
    {
        [ColumnName("Score")]
        public float L1CandidatesPerSelection { get; set; }
    }
}
