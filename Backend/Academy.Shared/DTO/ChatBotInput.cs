namespace Academy.Shared.DTO
{
    public class ChatBotInput
    {
        public int UserId { get; set; }
        public string Message { get; set; }
       // public ChatContext Context { get; set; }
    }
    public class TrainingAssignmentRequest
    {
        public string Email { get; set; }
        public List<TrainingInfo> TrainingList { get; set; }
    }
    //public class Employee
    //{
    //    public string Name { get; set; }
    //    public string Email { get; set; }
    //    public string TrainingNeeded { get; set; }
    //}

    public class ChatResponse
    {
        public string Reply { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }
        //public List<Employee> Employees { get; set; } = new List<Employee>();
        //public List<TrainingInfo> TrainingList { get; set; } = new List<TrainingInfo>();
    }
    public class TrainingInfo
    {
        public string TrainingName { get; set; }
        public string Status { get; set; }
        public bool Selected { get; set; }
       
    }
}