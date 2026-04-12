namespace Academy.Shared.DTO
{
    public record AuthenticatedUser
    {
        public int Id;
        public string GloberEmail = string.Empty;
        public string Name = string.Empty;
        public List<Role> Roles = [];
        public string Community = string.Empty;
        public string Ecosystem = string.Empty;
        public string CareerMentorEmail = string.Empty;
        public List<string> UserGexLeaderEmail = [];
        public string Project = string.Empty;
        public string Client = string.Empty;
        public short SeniorityId;
        public string Seniority = string.Empty;
        public bool IsAuthenticated = false;
        public List<string> GexLeaders = new();
    }
}
