namespace Academy.Domain.Entities;

public class DojoProjectConfiguration
{
    public int DojoProjectsConfigurationId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public bool IsAssignable { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
}