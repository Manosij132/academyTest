namespace Academy.Shared.DTO
{
    public class SpinTrainingRequest
    {
        public bool Force { get; set; } = false;
        public int Ecosystem { get; set; } = 0;
        public string Account { get; set; }
        public string TrainingAssignmentSrc { get; set; }
        public List<UserTrainingMapping> Mapping { get; set; } = new();
        public string[] SelectedTraning { get; set; } = [];

    }

    public class UserTrainingMapping
    {
        public int UserId { get; set; } = 0;
        public string UserEmail { get; set; } = "";
        public int SeniorityId { get; set; } = 0;
        public string Seniority { get; set; } = "";
        public string UserImage { get; set; } = "";
        public List<EcosystemTraining> Trainings { get; set; } = new();
        public bool Parent { get; set; } = false;
        public string[] SelectedTraning { get; set; } = [];
    }

    public class EcosystemTraining
    {
        public bool IsMvP { get; set; } = false;
        public string Seniority { get; set; } = "";
        public int SkillId { get; set; } = 0;
        public int SeniorityId { get; set; } = 0;
        public int TrainingCompletionHours { get; set; } = 0;
        public string TrainingDescription { get; set; } = "";
        public int TrainingId { get; set; } = 0;
        public string TrainingLink { get; set; } = "";
        public string TrainingName { get; set; } = "";
    }

    public class FetchEmployeesRequest
    {
        public string Startswith { get; set; }
        public short EcosystemId { get; set; }
        public string Account { get; set; }
    }
    public class FetchDojoGexRequest
    {
        public string Startswith { get; set; }
    }
}
