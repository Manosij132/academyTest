namespace Academy.Domain.Entities
{
    public class EmployeeCommunityMap : BaseEntity
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int CommunityId { get; set; }
    }
}
