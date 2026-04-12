namespace Academy.Domain.Entities
{
    public class CategoryMaster : BaseEntity
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; }
        public short? ParentCategoryId { get; set; }
    }
}
