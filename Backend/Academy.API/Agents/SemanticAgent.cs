using Academy.API.Models;
using Microsoft.SemanticKernel;

namespace Academy.API.Agents;

/// <summary>
/// SemanticAgent
/// </summary>
public abstract class SemanticAgent
{
    protected readonly Kernel Kernel;
    protected readonly string Name;
    protected readonly string Description;

    /// <summary>
    /// SemanticAgent
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    protected SemanticAgent(Kernel kernel, string name, string description)
    {
        Kernel = kernel;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// HandleAsync
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual async Task<AgentQueryResponse> HandleAsync(string input)
    {
        var arguments = new KernelArguments();
        arguments["input"] = input;
        var result = await ProcessInputAsync(arguments);

        return result;
    }

    /// <summary>
    /// ProcessInputAsync
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    protected abstract Task<AgentQueryResponse> ProcessInputAsync(KernelArguments arguments);

    /// <summary>
    /// GetMetadata
    /// </summary>
    /// <returns></returns>
    public virtual Dictionary<string, string> GetMetadata()
    {
        return new Dictionary<string, string>
        {
            ["name"] = Name,
            ["description"] = Description,
            ["version"] = "1.0.0"
        };
    }
}

/// <summary>
/// AgentResponse
/// </summary>
public class AgentResponse
{
    public AgentStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// AgentStatus
/// </summary>
public enum AgentStatus
{
    Success,
    Error,
    InputRequired
}
