using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Domain.Entities
{
    public class ProposedDojoGxLeader : BaseEntity
    {
        public int ProposedDojoGxLeaderId { get; set; }
        public int EmployeeId { get; set; }
        public string ProposedDojoLeaderEmailId { get; set; }
        public string GloberName { get; set; }
        public string ProposedLeaderName { get; set; }
        public string ProposedLeaderSeniority { get; set; }
        public string GloberSeniority { get; set; }
    }
}
