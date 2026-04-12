using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.DTO
{
    public class CommunitySelectionRatioModel
    {
        public string TDC { get; set; }
        public int CommunityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? L1SelectionRatio { get; set; }
        public decimal? GKSelectionRatio { get; set; }
    }
}
