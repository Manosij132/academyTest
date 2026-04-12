using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Domain.Entities
{
    public class EmployeeDocumentTypeMaster : BaseEntity
    {
        public byte EmployeeDocumentTypeId { get; set; }
        public string DocumentType { get; set; }
        public bool IsEligibleForReminder { get; set; } = false;
    }
}