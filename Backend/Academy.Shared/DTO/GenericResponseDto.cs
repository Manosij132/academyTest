
namespace Academy.Shared.DTO
{
    public class GenericResponseDto
    {
        public int Id { get; set; }
        public object Value { get; set; }
        public string Image { get; set; }
    }

    public class EcosystemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public int? PrimaryEcosystemId { get; set; }
    }
}
