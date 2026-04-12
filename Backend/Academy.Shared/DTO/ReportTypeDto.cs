using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.DTO
{
     
    public class ReportTypeDto
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; }
        public string StoredProcName { get; set; } = string.Empty;
        public bool IsGroupBy { get; set; }

    }
}
