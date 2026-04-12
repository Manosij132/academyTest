using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Services;
using Staffing.Core.Abstraction.Models;
using System.Reflection.Metadata;
using System.Diagnostics.Eventing.Reader;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Identity.Client;
using Academy.Shared.DTO;
using Staffing.Shared.DTO;
using Newtonsoft.Json;
using Academy.Shared.Constants;

namespace Staffing.API.Controllers
{
    [Route("api/semanticSearch")]
    [ApiController]
    public class SemanticSearchController(IAISettingsProvider settingsProvider, SqlServerDatabaseService sqlServerDatabaseService,
        AIService aiService, ILogger<SemanticSearchController> logger, ISemanticKernelService chatClientService) : BaseController
    {
        private readonly IAISettingsProvider _settingsProvider = settingsProvider;
        private readonly SqlServerDatabaseService _sqlServerDatabaseService = sqlServerDatabaseService;
        private readonly AIService _aiService = aiService;
        private readonly ISemanticKernelService _chatClientService = chatClientService;

        /// <summary>
        /// Structured search using query string.
        /// </summary>
        /// <param name="query">The query string to search.</param>
        /// <returns>A JSON result with search output or an error message.</returns>
        /// <response code="200">Returns the search result.</response>
        /// <response code="400">If the query is empty.</response>
        /// <response code="500">If a server or HTTP error occurs.</response>
        [HttpGet("staffing")]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> GetStaffingSearchResult([FromQuery] string query)
        {
            object result;
            try
            {
                var aiConnection = _settingsProvider.GetAIConnection();
                result = await GetResult(query, aiConnection, aiConnection.StaffingDbConnection, StructuredAgent.Staffing);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"{Messages.ERROR_AIAgentLogErrorPrefix} {query}");
                return Ok(JsonConvert.SerializeObject(new StaffingServiceResponse() { Reply = $"{Messages.ERROR_AIAgentLogErrorMessage}" }));
            }
            return Ok(JsonConvert.SerializeObject(result));
        }

        private async Task<IActionResult> GetResult(string query, AIConnection aiConnection, DataConnection dbConnection, StructuredAgent structuredAgent)
        {
            var conversationId = Request.Headers["conversationid"];

            // inform semantic kernel service of the session
            _chatClientService.SetSessionId(conversationId);
            var history = _chatClientService.LoadChatHistoryFromDB();

            StaffingServiceResponse staffingServiceResponse = new StaffingServiceResponse();
            List<Dictionary<string, string>> response = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> res = new List<Dictionary<string, string>>();
            int tryCount = 3;
            while (true)
            {
                try
                {
                    logger.LogInformation($"Prompt: {query}");

                    var dbSchema = await _sqlServerDatabaseService.GenerateSchema(dbConnection);

                    List<SuggestedQuestionsDTO> suggestionQueries = new List<SuggestedQuestionsDTO>();
                    AIQuery aiQuery = new AIQuery();
                    if (history is null)
                    {
                        suggestionQueries = await _aiService.GenerateClarifyingQuestions(query, dbSchema, _chatClientService);
                    }
                    //else { 
                    aiQuery = await _aiService.GetAISQLQuery(
                        aiModel: _settingsProvider.GetAIModel(),
                        aiService: _settingsProvider.GetAIService(),
                        userPrompt: query,
                        dbSchema: dbSchema,
                        databaseType: aiConnection.AttendenceDbConnection.DatabaseType,
                        structuredAgent: structuredAgent
                    );
                    //}

                    if (suggestionQueries.Count == 0)
                    {
                        suggestionQueries = await _aiService.GenerateClarifyingQuestions(query, dbSchema, _chatClientService);

                        response = await _sqlServerDatabaseService.GetDataTable(dbConnection, aiQuery.query);
                        var summary = await _aiService.GetSummary(aiQuery.query, dbSchema, _chatClientService);
                        staffingServiceResponse.Data = response;
                        staffingServiceResponse.Type = "table";
                        staffingServiceResponse.SuggestedPromt = suggestionQueries;
                        staffingServiceResponse.Reply = response.Count > 0 ? summary : "Hmm, I did not find any matches. Would you like to refine your request?";
                    }
                    else
                    {
                        staffingServiceResponse.Data = response;
                        staffingServiceResponse.SuggestedPromt = suggestionQueries;
                        staffingServiceResponse.Type = "message";
                        staffingServiceResponse.Reply = "Here are some of the clarifying questions ";
                    }

                    if (response.Any() || suggestionQueries.Count > 0 || --tryCount == 0)
                        break; // success!

                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    logger.LogInformation($"Status: Failed, no data found., Retrying Count: {tryCount}");
                }
                catch (Exception ex)
                {
                    if (--tryCount == 0)
                        throw;
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    logger.LogError(ex, ex.Message);
                }
            }
            return Ok(staffingServiceResponse);
        }

    }
}


public class StaffingServiceResponse(string? errorMessage = null)
{
    public object? Data { get; set; }
    public object? SuggestedPromt { get; set; }
    public string? ErrorMessage { get; set; } = errorMessage;
    public string? Reply { get; set; }
    public string? Type { get; set; }
}
