using Staffing.Core.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Infrastructure
{
    public interface IDatabaseService
    {
        Task<List<Dictionary<string, string>>> GetDataTable(DataConnection conn, string sqlQuery);
        Task<DatabaseSchema> GenerateSchema(DataConnection conn);
    }
}
