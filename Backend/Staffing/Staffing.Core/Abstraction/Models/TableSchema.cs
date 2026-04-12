using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Models
{
    public class TableSchema()
    {
        public string TableName { get; set; }
        public List<string> Columns { get; set; }
    }
}
