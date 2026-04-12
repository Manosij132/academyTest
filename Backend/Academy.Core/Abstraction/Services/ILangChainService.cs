using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Academy.Core.Services;
using Academy.Shared.DTO;

namespace Academy.Core.Abstraction.Services
{
    public interface ILangChainService
    {
         Task<LangChainResponse> ProcessUserInputAsync(string input);
    }
}
