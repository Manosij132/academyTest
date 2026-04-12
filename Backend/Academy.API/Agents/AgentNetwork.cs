using Academy.API.Agents;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Academy.API.Models;

/// <summary>
/// AgentNetwork
/// </summary>
public class AgentNetwork
{
    private readonly Dictionary<string, string> _agents;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// AgentNetwork
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="httpClient"></param>
    /// <param name="options"></param>
    public AgentNetwork(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IOptions<AgentOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
        _agents = options.Value.Agents.ToDictionary(x => x.Name.ToLowerInvariant(), x => x.Endpoint);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool HasAgent(string name)
    {
        var agents = name.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

        foreach (var agent in agents)
        {
            if (!_agents.ContainsKey(agent.ToLowerInvariant()))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="agentQueryString"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public List<AgentQuery> GetParsedAgentQueryList(string agentQueryString)
    {
        try
        {
            return JsonSerializer.Deserialize<List<AgentQuery>>(agentQueryString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Router returned invalid JSON.", ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string? GetAgentEndpoint(string name)
    {
        return _agents.TryGetValue(name.ToLowerInvariant(), out var endpoint) ? endpoint : null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAgentNames()
    {
        return _agents.Keys;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="agentName"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<AgentQueryResponse> QueryAgentAsync(string agentName, string query)
    {
        var endpoint = GetAgentEndpoint(agentName);
        if (endpoint == null)
        {
            throw new ArgumentException($"Agent '{agentName}' not found in the network");
        }

        var apiEndpoint = endpoint + System.Net.WebUtility.UrlEncode(query);

        var accessToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        var conversationid = _httpContextAccessor.HttpContext?.Request.Headers["conversationid"].ToString();

        // --- Retry Logic for failed responses ---
        const int maxRetries = 2;
        string result = string.Empty;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);

            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Replace("Bearer ", ""));
            }

            if (!string.IsNullOrEmpty(conversationid))
            {
                _httpClient.DefaultRequestHeaders.Remove("conversationid");
                _httpClient.DefaultRequestHeaders.Add("conversationid", conversationid);
            }

            //await ModifyRequestForAcademyAgent(agentName);
            HttpResponseMessage response=null;
            
            response = await _httpClient.SendAsync(request);
           
            if (response.StatusCode != HttpStatusCode.OK && attempt < maxRetries)
            {
                await Task.Delay(200); // short delay before retry
                continue;
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                result = "Hmm, I didn’t find any matches. Would you like to refine your request?";
                return CreateAgentQueryResponse(agentName, query, result);
            }

            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadAsStringAsync();
            

            if (!string.IsNullOrWhiteSpace(result) || attempt == maxRetries)
                break;

            await Task.Delay(200); // retry if empty body
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            result = "Hmm, I didn’t find any matches. Would you like to refine your request?";
        }

        return CreateAgentQueryResponse(agentName, query, result);
    }

    private AgentQueryResponse CreateAgentQueryResponse(string agent, string query, string result)
    {
        return new AgentQueryResponse
        {
            Agent = agent,
            Query = query,
            Response = result
        };
    }

    private async Task ModifyRequestForAcademyAgent(string agentName)
    {
        if (agentName.ToLower() == "academy")
        {
            var authResponse = await _httpClient.GetAsync("http://localhost:5160/api/account/authenticate");

            if (authResponse.IsSuccessStatusCode)
            {
                var responseContent = await authResponse.Content.ReadAsStringAsync();

                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("data", out var tokenElement))
                {
                    var authToken = tokenElement.GetString();

                    if (!string.IsNullOrEmpty(authToken))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", authToken.Replace("Bearer ", ""));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="agentName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Dictionary<string, string>> GetAgentMetadataAsync(string agentName)
    {
        var endpoint = GetAgentEndpoint(agentName);
        if (endpoint == null)
        {
            throw new ArgumentException($"Agent '{agentName}' not found in the network");
        }

        var apiEndpoint = endpoint.TrimEnd('/') + "/api/metadata";
        var response = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(apiEndpoint);
        return response ?? new Dictionary<string, string>();
    }
}

/// <summary>
/// 
/// </summary>
public class AgentResponse
{
    /// <summary>
    /// 
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 
/// </summary>
public class AgentQueryResponse
{
    /// <summary>
    /// 
    /// </summary>
    public string Agent { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Query { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string  Response { get; set; } = string.Empty;

}

/// <summary>
/// 
/// </summary>
public class AgentQuery
{
    /// <summary>
    /// 
    /// </summary>
    public string Agent { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Query { get; set; } = string.Empty;
}
