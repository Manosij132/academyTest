using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Models
{
    public class DatabaseSchema
    {
        public List<TableSchema> SchemaStructured { get; set; }
        public List<KeyValuePair<string, string>> SchemaRaw { get; set; }
    }
}
