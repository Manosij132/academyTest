using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.DTO
{
    public class TrainingUpdateRequest
    {
        public string TrainingName { get; set; }
        public string EmployeeEmail { get; set; }
        public string TrainingStatus { get; set; }
        public string SkillName { get; set; }
        public string EmployeeName { get; set; }
        public string EcoSystem { get; set; }
    }

    public class EmployeeDetailsRequest
    {
        public string[] EmployeeEmail { get; set; }
        public string[] EmployeeName { get; set; }
    }
}
