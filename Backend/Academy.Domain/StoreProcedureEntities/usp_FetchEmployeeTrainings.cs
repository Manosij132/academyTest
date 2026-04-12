using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Domain.StoreProcedureEntities
{
    public class usp_FetchEmployeeTrainings
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public short SkillId { get; set; }
        public int TrainingId { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public byte TrainingStatusId { get; set; }
        public string TrainingStatus { get; set; }
        public int EmployeeTrainingMapId { get; set; }
        public string SkillName { get; set; }
        public string TrainingName { get; set; }
        public string TrainingUrl { get; set; }
        public bool IsMVP { get; set; }
    }
}
