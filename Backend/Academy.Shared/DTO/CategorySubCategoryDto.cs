namespace Academy.Shared.DTO
{
    public class CategoryDto
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; } = [];
    }

    public class SubCategoryDto
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public short ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}
