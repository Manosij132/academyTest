using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Infrastructure
{
    public interface IChatClientService
    {
        Services.LLModelResponse GetLLModelConfiguration(CancellationToken cancellationToken = default);

        Task<string> GetResponseAsync(Microsoft.SemanticKernel.ChatCompletion.ChatHistory chatMessages, CancellationToken cancellationToken = default);
    }
}
