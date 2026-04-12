using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Infrastructure
{
    public interface ISemanticKernelService
    {
        Kernel GetSemanticKernel();
        Task<string> GetResponseAsync(Microsoft.SemanticKernel.ChatCompletion.ChatHistory chatMessages, CancellationToken cancellationToken = default);

        Task<List<string>> GenrateSuggestedPromt(string input, object dbData);

        ChatHistory? LoadChatHistoryFromDB();

        Task<string> GenerateSuggestedPrompt(string input, object dbData);

        void SetSessionId(string? sessionId);
    }
}
