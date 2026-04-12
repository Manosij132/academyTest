using Academy.API.Models;
using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Shared;
using Academy.Infrastructure.EF;
using Academy.Shared.Constants;
using Academy.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
namespace Academy.API.Agents;

/// <summary>
/// 
/// </summary>
public class RouterAgent : SemanticAgent
{
    private readonly KernelFunction _routerFunction;
    private readonly AgentNetwork _network;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="network"></param>
    public RouterAgent(Kernel kernel, AgentNetwork network)
        : base(kernel, "Router Agent", "Routes queries to specialized agents")
    {
        _network = network;
      
        // Load the router prompt as a semantic function
        _routerFunction = kernel.CreateFunctionFromPrompt(
            File.ReadAllText("Prompts/RouterPrompt.txt"),
            new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    { "temperature", 0.0 }, // Use 0 temperature for deterministic routing
                    { "top_p", 1.0 }
                }
            }
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    protected override async Task<AgentQueryResponse> ProcessInputAsync(KernelArguments arguments)
    {
        var query = arguments["input"]?.ToString() ?? string.Empty;

        // Get routing decision from the LLM
        var routingResult = await Kernel.InvokeAsync(_routerFunction, arguments);
        var targetAgentsQuerys = routingResult.GetValue<string>()?.Trim() ?? string.Empty;

        // Get json array string parsed into list
        List<AgentQuery>? agents = _network.GetParsedAgentQueryList(targetAgentsQuerys);

        if (agents is null || agents.Count == 0 || !agents.All(agent => _network.HasAgent(agent.Agent)))
        {
            throw new CustomException($"No suitable agent found to handle the query: {query}");
        }

        // Forward the query to the selected multiple agents
        if (agents.Count > 1)
        {
            var tasks = agents.Select(agentObj => _network.QueryAgentAsync(agentObj.Agent.ToLowerInvariant(), agentObj.Query));

            var responses = await Task.WhenAll(tasks);

            var consolidationPrompt = @$"
            You are a Orchestrator responsible for consolidating information from multiple specialized agents.

            Your task:
            - Carefully review the responses from each agent below.
            - Combine them into a single well-structured response.
            - If responses are about different topics, group them into separate sections using valid HTML tags (use <h2> or <h3> for section headers).
            - Preserve the key details and formatting from the agent responses without rewriting unnecessarily.
            - Do NOT use Markdown, ###, or * characters.
            - Return ONLY a valid HTML string as the final output.

            Important formatting rules:
            - Only use table formatting if the response data is actually tabular (e.g., multiple rows with similar fields).
            - When creating a table, use <table>, <thead>, <tbody>, <tr>, <th>, <td>.
            - Make sure <th> and <td> are left-aligned for consistency.
            - Apply minimal table styling for readability:
              <table style='border-collapse: collapse; width: 100%;'>
              <thead><tr><th style='text-align:left; border-bottom:1px solid #ccc;'>...</th></tr></thead>
              <tbody><tr><td style='text-align:left; padding:4px;'>...</td></tr></tbody>
            - For simple key-value data (like a single name or email), just use <p> or <div>, do not wrap it in a table.

            Agent Responses:
            {string.Join("\n\t\t\t", responses.Select(r => $"Agent: {r.Agent}; Query: {r.Query}; Response: {r.Response}"))}

            Original User Request: {query}

            Generate a clear, concise, and well-organized consolidated response in HTML format.
            Format the response in a user-friendly way, grouping related information together.
            ";

            var result = await Kernel.InvokePromptAsync(consolidationPrompt);

            return new AgentQueryResponse() { Agent = agents.FirstOrDefault().Agent, Response = result.ToString(), Query = query };
        }
        //Forward the query to selected single agent
        else
        {
            var response = await _network.QueryAgentAsync(agents.First().Agent.ToLowerInvariant(), query);

            return response;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<(string AgentName, float Confidence)> RouteQueryAsync(string query)
    {
        try
        {
            var arguments = new KernelArguments();
            arguments["input"] = query;

            var result = await Kernel.InvokeAsync(_routerFunction, arguments);
            var agentName = result.GetValue<string>()?.Trim().ToLowerInvariant() ?? string.Empty;

            // For now, we're using a fixed confidence score since we're using temperature 0
            // In a more sophisticated implementation, we could use the LLM's confidence scores
            return (agentName, 1.0f);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="network"></param>
    /// <param name="modelId"></param>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    public static RouterAgent Create(AgentNetwork network,IAcademyDbContext _academyDbContext,string? modelId = null, string? apiKey = null,string? environment = "dev")
    {
        var builder = Kernel.CreateBuilder();

        var _environment = environment ?? "dev";

        var keyName = apiKey.TrimStart('#').TrimEnd('#');
        // Fetch the value from the database
        var dbValue = string.Empty;

        var configEntity =  _academyDbContext.Configurations
                                            .FirstOrDefaultAsync(x => x.Environment.ToLower() == _environment.ToLower() &&
                                                                        x.Key.ToLower() == keyName.ToLower());

        if (configEntity != null)
        {
            dbValue = configEntity.Result.Value;
        }
        else
        {
            throw new Exception(Messages.ERROR_CONFIG_KEY_NOT_FOUND);
        }

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.AddOpenAIChatCompletion(
                modelId: modelId ?? "openai/gpt-4o-mini",
                endpoint: new Uri("https://api.saia.ai/chat"),
                apiKey: dbValue.Decrypt()
            );
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        //builder.AddOpenAIChatCompletion(
        //         modelId: modelId ?? "openai/gpt-4o-mini",
        //         apiKey: apiKey
        //     );
        return new RouterAgent(builder.Build(), network);
    }
    
}
