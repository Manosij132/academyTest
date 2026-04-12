using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.DTO
{
    public class UpdateGxLeader
    {
        public int? DojoDetailId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DojoStartDate { get; set; }
        public DateTime? DojoEndDate { get; set; }
        public string DojoGxLeaderEmail { get; set; }
        public string DojoGxGlobarEmail { get; set; }
        public string GloberName { get; set; }
        public string ProposedLeaderName { get; set; }
        public string ProposedLeaderSeniority { get; set; }
        public string GloberSeniority { get; set; }

    }

    public class UpdateMentees
    {
        public int? DojoDetailId { get; set; }
        public List<int> EmployeeId { get; set; }
        public DateTime DojoStartDate { get; set; }
        public DateTime? DojoEndDate { get; set; }
        public string DojoGxLeaderEmail { get; set; }
        public string DojoGxGlobarEmail { get; set; }
        public string GloberName { get; set; }
        public string ProposedLeaderName { get; set; }
        public string ProposedLeaderSeniority { get; set; }
        public string GloberSeniority { get; set; }

    }
}
