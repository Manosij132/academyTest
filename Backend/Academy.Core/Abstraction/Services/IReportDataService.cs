using Academy.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Core.Abstraction.Services
{
    public interface IReportDataService
    {
        Task<dynamic> GetReportData(BookMarkRequest request, bool fromRequest = false);
    }
}
