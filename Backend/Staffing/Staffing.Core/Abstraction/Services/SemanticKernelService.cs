using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Staffing.Core.Abstraction.Infrastructure;
using System.Text.Json;

namespace Staffing.Core.Abstraction.Services
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly Kernel _kernel;
        private readonly IChatClientService _chatClientService;
        private static PromptExecutionSettings? settings;
        private readonly ILogger<SemanticKernelService> _logger;
        private readonly IChatHistoryRepository? _chatHistoryRepository;
        private string? _currentSessionId;

        public SemanticKernelService(IChatClientService chatClientService, ILogger<SemanticKernelService> logger, IChatHistoryRepository? chatHistoryRepository = null)
        {
            _chatClientService = chatClientService;
            _logger = logger;
            _kernel = BuildKernel();
            _chatHistoryRepository = chatHistoryRepository;
            
        }

        // Called by per-request controller (or a higher-level service) to set the session id for this request
        public void SetSessionId(string? sessionId)
        {
            _currentSessionId = string.IsNullOrWhiteSpace(sessionId) ? null : sessionId;
        }

        private Kernel BuildKernel()
        {
            var config = Task.Run(async () => _chatClientService.GetLLModelConfiguration()).GetAwaiter().GetResult();
            var provider = config.AIProvider.ToLowerInvariant();
            Kernel kernel;
            switch (provider)
            {
                case "ge-openai":
                    kernel = Kernel.CreateBuilder()
                                   .AddOllamaEmbeddingGenerator("mxbai-embed-large", new Uri("http://10.221.85.55:11434"))
                                   .AddOpenAIChatCompletion(config.AIModel, new Uri(config.Endpoint), config.ApiKey)
                                   .Build();
                    settings = new OpenAIPromptExecutionSettings()
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                    };
                    break;
                case "openai":
                    kernel = Kernel.CreateBuilder()
                                   .AddOpenAIChatCompletion(config.AIModel, config.ApiKey!)
                                   .Build();
                    settings = new OpenAIPromptExecutionSettings()
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                    };
                    break;
                case "ollama":
                    kernel = Kernel.CreateBuilder()
                                   .AddOllamaChatCompletion(config.AIModel, new Uri(config.Endpoint))
                                   .AddOllamaEmbeddingGenerator("mxbai-embed-large", new Uri(config.Endpoint))
                                   .Build();
                    settings = new OllamaPromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        Temperature = 0.1f
                    };
                    break;
                case "azureopenai":
                    kernel = Kernel.CreateBuilder()
                                   .AddAzureOpenAIChatCompletion(config.DeploymentName!, config.Endpoint, config.ApiKey!, config.ServiceId, config.AIModel)
                                   .Build();
                    break;
                default: throw new NotImplementedException("No suitable ai provider configured.");
            }
            _logger.LogInformation($"Semantic kernel has been configured using Provider:{config.AIProvider} and Model:{config.AIModel}");
            return kernel;
        }

        public Kernel GetSemanticKernel() => _kernel;


        public async Task<string> GetResponseAsync(Microsoft.SemanticKernel.ChatCompletion.ChatHistory chatMessages, CancellationToken cancellationToken = default)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            var response = await chatCompletionService.GetChatMessageContentAsync(chatMessages, settings, _kernel, cancellationToken);

            string assistantReply = response.Content.ToString();
            
            // delete previous storage snapshot (file or DB) — keep behavior (clear before save)
            await DeleteChatHistoryAsync(cancellationToken).ConfigureAwait(false);
            await SaveChatHistoryAsync(chatMessages, cancellationToken).ConfigureAwait(false);
            return assistantReply;
        }

        private async Task SaveChatHistoryAsync(ChatHistory chatMessages, CancellationToken cancellationToken = default)
        {
            // Convert to DTO-like list similar to previous file behavior
            var list = chatMessages.Select(msg => new Academy.Shared.DTO.ChatMessage
            {
                Role = msg.Role.ToString(),
                Content = msg.Content,
            }).ToList();

            if (!string.IsNullOrEmpty(_currentSessionId) && _chatHistoryRepository != null)
            {
                // save to DB for this session
                await _chatHistoryRepository.SaveAsync(_currentSessionId, list, cancellationToken);
                return;
            }

         }

        private async Task DeleteChatHistoryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentSessionId) && _chatHistoryRepository != null)
                {
                    await _chatHistoryRepository.DeleteBySessionAsync(_currentSessionId, cancellationToken).ConfigureAwait(false);
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete chat history.");
            }
        }

        public ChatHistory? LoadChatHistoryFromDB()
        {
            // Prefer DB-backed history scoped to session + user when repository is available
            if (!string.IsNullOrEmpty(_currentSessionId) && _chatHistoryRepository != null)
            {
                try
                {
                    var dbHistory = _chatHistoryRepository.LoadAsync(_currentSessionId).GetAwaiter().GetResult();
                    if (dbHistory != null)
                        return dbHistory;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load chat history from DB for session {SessionId}. Falling back to file.", _currentSessionId);
                }
            }

            return null;
        }

        public async Task<List<string>> GenrateSuggestedPromt(string input, object dbData)
        {
            var response = await _kernel.InvokePromptAsync($@"
                User asked: 
                            {input}
                DB result: {JsonSerializer.Serialize(dbData)}");

            return response.ToString()
                        .Split('\n')
                        .Where(q => !string.IsNullOrWhiteSpace(q))
                        .Take(6)
                        .ToList();
        }
        public async Task<string> GenerateSuggestedPrompt(string input, object dbData)
        {
            var response = await _kernel.InvokePromptAsync($@"
                User asked: 
                            {input}
                DB result: {JsonSerializer.Serialize(dbData)}");

            return response.ToString();
        }

    }
}
