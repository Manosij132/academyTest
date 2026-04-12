using Academy.API.Agents;
using Academy.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Academy.API.Controllers
{
    /// <summary>
    /// SemanticSearchController
    /// </summary>
    
    [ApiController]
    [Route("api/semanticSearch")]
    public class SemanticSearchController(ILogger<SemanticSearchController> logger, AgentNetwork _network) : ControllerBase
    {
        /// <summary>
        /// Sends a message with context and streams the response.
        /// </summary>
        /// <param name="agent">The router agent.</param>
        /// <param name="query">The chat message request object.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>An empty result on success or an error message on failure.</returns>
        /// <response code="200">Returns an empty result indicating success.</response>
        /// <response code="400">If the message is null or empty.</response>
        /// <response code="500">If an error occurs while processing the request.</response>
        [HttpGet("GetReply")]
        //[ProducesResponseType(typeof(void), 200)]
        //[ProducesResponseType(typeof(ProblemDetails), 400)]
        //[ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> StreamChatResponse(RouterAgent agent, [FromQuery] string query, CancellationToken cancellationToken)
        {
            AgentQueryResponse response = null;
            try
                {
                var (agentNames, confidence) = await agent.RouteQueryAsync(query);
                // Get json array string parsed into Agnet-Query list
                var agentQueryList = _network.GetParsedAgentQueryList(agentNames);
                var agents = string.Join(", ", agentQueryList.Select(a => a.Agent));
                logger.LogInformation($"A new query was received: {query}. The identified agent(s) are {agents}, with a confidence score of {confidence}.");

                response = await agent.HandleAsync(query);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"AI agent failed to answer for question: {query}");
                return Ok(new AgentQueryResponse() {  Response = "The AI agents are having trouble communicating. This may cause delays in processing your request.</br> Please bear with us while this is resolved. </br></br>Would you like to try something else?", Query = query });
            }
       
            return Ok(response);
        }
    }
}
