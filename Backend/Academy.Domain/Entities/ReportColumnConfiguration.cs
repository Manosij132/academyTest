using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Domain.Entities
{
     
    public class ReportColumnConfiguration : BaseEntity
    {
        public int ReportColumnConfigId { get; set; }
        public string ReportColumnName { get; set; }
        public string ReportColumnDisplayName { get; set; } = string.Empty;
        public bool IsGroupBy { get; set; }

    }
}
