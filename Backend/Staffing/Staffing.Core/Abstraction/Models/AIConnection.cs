using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Models
{
    public class AIConnection
    {
        public DataConnection EmployeeDbConnection { get; set; } = default!;
        public DataConnection AttendenceDbConnection { get; set; } = new DataConnection ("Data Source=IN-IT19689\\SQLEXPRESS;Initial Catalog=glober-db;Trusted_Connection=True;TrustServerCertificate=true;", "MSSQL", ["StaffRequests", "Plans", "RequestSkills"]);
        public DataConnection StaffingDbConnection { get; set; } = default!;
        public string AIModel { get; set; } = default!;
        public string AIService { get; set; } = default!;
        public DataConnection? ClientDbConnection { get; set; }
    }
    public record DataConnection(string ConnectionString, string DatabaseType, string[] Entities);
}
