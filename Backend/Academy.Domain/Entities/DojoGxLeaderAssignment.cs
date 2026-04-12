using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Domain.Entities
{
    public class DojoGxLeaderAssignment : BaseEntity
    {
        public int DojoGxLeaderAssignmentId { get; set; }
        public int DojoDetailId { get; set; }
        public DateTime AssignmentStartDate { get; set; }
        public DateTime? AssignmentEndDate { get; set; }
        public string LeaderEmail { get; set; }
        public string Comments { get; set; }
    }
}
