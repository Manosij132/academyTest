using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.DTO
{
    public class SpinTrainingState
    {
        public string EcoSystem { get; set; }
        public string[] EmployeeEmail { get; set; }
        public string[] EmployeeName { get; set; }
        public int[] TrainingIds { get; set; }
        public string Account { get; set; }
        public string TrainingSource { get; set; }

        public string TrainingName { get; set; }

        public string SpinBasedOnAccount { get; set; }

        public string IsForceAssign { get; set; }

        public string ForAllEmployees { get; set; } // "yes" or "no"
    }
}
