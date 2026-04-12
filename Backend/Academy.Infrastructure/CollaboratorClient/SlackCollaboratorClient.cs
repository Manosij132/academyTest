using Academy.Core.Abstraction.Infrastructure;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Academy.Shared.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace Academy.Infrastructure.CollaboratorClient
{
    public class SlackCollaboratorClient : ICollaboratorClient
    {
        private readonly AppSetting _appSetting;
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackCollaboratorClient(IOptions<AppSetting> appSetting, IHttpClientFactory httpClientFactory)
        {
            _appSetting = appSetting.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendMessageAsync(dynamic message)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appSetting.SlackBotToken);

            var payload = new
            {
                channel = _appSetting.SlackChannelId,
                text = message
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://slack.com/api/chat.postMessage", content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to send message to Slack. Status code: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {responseContent}");
            }
            else
            {
                Console.WriteLine("Message sent successfully!");
            }
        }
    }
}
