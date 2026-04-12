using Staffing.Core.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Infrastructure
{
    public interface IAISettingsProvider
    {
        AIConnection GetAIConnection();
        string GetAIModel();
        string GetAIService();
    }
}
