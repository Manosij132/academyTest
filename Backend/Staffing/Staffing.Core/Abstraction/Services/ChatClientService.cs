using Academy.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Staffing.Core.Abstraction.Infrastructure;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;

namespace Staffing.Core.Abstraction.Services
{
    public class ChatClientService : IChatClientService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatClientService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public LLModelResponse GetLLModelConfiguration(CancellationToken cancellationToken = default)
        {                  

            var llmConfiguration =  _configuration
            .GetSection("LLMSettings")
            .Get<LLModelResponse>();

            if (llmConfiguration == null)
            {
                throw new ArgumentException(nameof(llmConfiguration));
            }   
            return llmConfiguration;
        }

        public async Task<string> GetResponseAsync(Microsoft.SemanticKernel.ChatCompletion.ChatHistory chatMessages, CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var endpoint = _configuration.GetValue<string>("LLM_ENDPOINT");
            if (endpoint == null)
            {
                throw new ArgumentException(nameof(endpoint));
            }

            var accessToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Replace("Bearer ", ""));
            }


            string jsonContent = JsonConvert.SerializeObject(chatMessages);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<string>(result)!;
        }
    }

    public class LLModelResponse
    {
        public string AIProvider { get; set; } = default!;
        public string AIModel { get; set; } = default!;
        public string Endpoint { get; set; } = default!;
        public string? ApiKey { get; set; } = default!;
        public string? DeploymentName { get; set; } = default!;
        public string? ServiceId { get; set; } = default!;
    }
}
