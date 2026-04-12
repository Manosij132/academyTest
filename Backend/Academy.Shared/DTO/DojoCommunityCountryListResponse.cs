
namespace Academy.Shared.DTO
{
    public class DojoCommunityCountryListResponse
    {
        public List<string> Countries { get; set; }
        public List<string> Communities { get; set; }
        public List<string> AiStudios { get; set; }
        public List<AiStudioAccount> Accounts { get; set; }        
    }

    public class AiStudioAccount
    {
        public string AiStudio { get; set; }
        public string Account { get; set; }
    }
}