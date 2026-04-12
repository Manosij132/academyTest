using Academy.API.Agents;
using Academy.API.Models;
using Academy.Core.Abstraction.Services;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace Academy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    public class ChatBotController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private ILangChainService _langChainService;
        private AgentNetwork _network;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ILogger<ChatBotController> _logger;
        public ChatBotController(IDashboardService dashboardService, IAuthenticatedUserService authenticatedUserService, ILangChainService langChainService, AgentNetwork network, ILogger<ChatBotController> logger)
        {
            _dashboardService = dashboardService;
            _langChainService = langChainService;
            _network = network;
            _authenticatedUserService = authenticatedUserService;
            _logger = logger;
        }

        [HttpPost("GetReply")]
        public async Task<ActionResult<ChatResponse>> GetReply([FromBody] ChatBotInput input)
        {
            var response = new ChatResponse();
            // var a = _chatboartService.ExecuteChatBotTrainingAssignment("vasant.parmar@globant.com", "dd");
            if (string.IsNullOrWhiteSpace(input?.Message))
            {
                response.Reply = "Input message is empty.";
                return Ok(response);
            }
            var result = await _langChainService.ProcessUserInputAsync(input?.Message);
            response.Reply = result.Message.ToString();
            if (result.Message.Contains("Exception"))
            {
                response.Reply = "Did not get you. Can you please be more precise?";
            }
            response.Data = result.Data;
            response.Type = result.Type;
            return Ok(response);
        }

        [HttpGet("GetAcademyData")]
        public async Task<ActionResult<ChatResponse>> GetAcademyData([FromQuery] string query)
        {
            var response = new ChatResponse();
            try
            {
                // var a = _chatboartService.ExecuteChatBotTrainingAssignment("vasant.parmar@globant.com", "dd");
                if (string.IsNullOrWhiteSpace(query))
                {
                    response.Reply = "Input message is empty.";
                    return Ok(response);
                }
                var result = await _langChainService.ProcessUserInputAsync(query);
                response.Reply = result.Message.ToString();
                if (result.Message.Contains("Exception"))
                {
                    response.Reply = "Did not get you. Can you please be more precise?";
                }
                response.Data = result.Data;
                response.Type = result.Type;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"{Messages.ERROR_AIAgentLogErrorPrefix} {query}");
                return Ok(new ChatResponse() { Reply = $"{Messages.ERROR_AIAgentLogErrorMessage}" });
            }
            return Ok(response);
        }


        [HttpGet("GetReply")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> StreamChatResponse(RouterAgent agent, [FromQuery] string query, CancellationToken cancellationToken)
        {
            string response = null;
            try
            {
                var (agentNames, confidence) = await agent.RouteQueryAsync(query);
                var SysAdmin = _authenticatedUserService.AuthUser.Roles.FirstOrDefault(x => x.RoleId.Equals((int)Roles.SystemAdmin));
                
                // Get json array string parsed into Agnet-Query list
                var agentQueryList = _network.GetParsedAgentQueryList(agentNames);
                var agents = string.Join(", ", agentQueryList.Select(a => a.Agent));
                //logger.LogInformation($"A new query was received: {query}. The identified agent(s) are {agents}, with a confidence score of {confidence}.");

                if (agents == AgentNames.staffing.ToString() && SysAdmin == null)
                {
                    return Ok(new AgentQueryResponse() { Response = Messages.ERROR_InSufficientPermissions, Query = query });
                }

                response = JsonConvert.SerializeObject(agents);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{Messages.ERROR_AIAgentLogErrorPrefix} {query}");
                return Ok(new AgentQueryResponse() { Response = $"{Messages.ERROR_AIAgentLogErrorMessage}", Query = query });
            }

            return Ok(response);
        }

        [HttpPost("AssignTrainings")]
        public IActionResult AssignTrainings([FromBody] TrainingAssignmentRequest request)
        {
            var response = new ChatResponse();

            if (request?.TrainingList == null || !request.TrainingList.Any(t => t.Selected))
            {
                response.Reply = "No trainings were selected." + request.Email;
            }
            else
            {
                var selectedTrainings = request.TrainingList
                    .Where(t => t.Selected)
                    .Select(t => t.TrainingName)
                    .ToList();

                // You can store it in DB or just return confirmation
                response.Reply = $"Trainings have been successfully assigned to {request.Email ?? "the user"}: {string.Join(", ", selectedTrainings)}";
            }

            return Ok(response);
        }

        [HttpPost("spin/AssignTrainings")]
        public async Task<IActionResult> InitSpinTraining([FromBody] SpinTrainingRequest request)
        {
            var result = await _dashboardService.ExecuteTrainingAssignmentJob(request);
            string trnxId = result.Value;
            AcademyResponse<string> response = new()
            {
                Data = trnxId,
                Status = HttpStatusCode.OK,
                Success = true
            };
            return Ok(response);
        }

        [HttpGet("academySearch")]
        public async Task<ActionResult<ChatResponse>> GetReply([FromQuery] string query)
        {
            var response = new ChatResponse();
            var result = await _langChainService.ProcessUserInputAsync(query);
            response.Reply = result.Message.ToString();
            if (result.Message.Contains("Exception"))
            {
                response.Reply = "Did not get you. Can you please be more precise?";
            }
            response.Data = result.Data;
            response.Type = result.Type;
            return Ok(response.Data);
        }
    }

}