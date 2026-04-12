using Academy.Shared.Extensions;
namespace Academy.Core.Models
{
    public class AcademyDbSetting : IAdoSetting
    {
        public string ConnectionString { get; set; }
        public AcademyDbSetting(string encryptedConnectionString)
        {
            ConnectionString = encryptedConnectionString.Decrypt();
        }
    }
}
