namespace Academy.API.Agents
{
    /// <summary>
    /// Agent options
    /// </summary>
    public class AgentOptions
    {
        public List<Agents> Agents { get; set; } = default!;
    }

    /// <summary>
    /// Agents
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Endpoint"></param>
    public record Agents(string Name, string Endpoint);
}
