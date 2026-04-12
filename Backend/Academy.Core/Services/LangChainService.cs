using Academy.API.Helpers;
using Academy.Core.Abstraction.Services;
using Academy.Core.Shared;
using Academy.Core.Utilities;
using Academy.Shared.Constants;
using Academy.Shared.DTO;
using Academy.Shared.Extensions;
using LangChain.Chains.HelperChains.Exceptions;
using LangChain.Chains.StackableChains.Agents.Crew;
using LangChain.Chains.StackableChains.Agents.Crew.Tools;
using LangChain.Memory;
using LangChain.Prompts;
using LangChain.Providers;
using LangChain.Providers.Ollama;
using LangChain.Providers.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Ollama;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LangChain.Prompts.PromptTemplate;


namespace Academy.Core.Services
{
    public class LangChainService : ILangChainService
    {
        private readonly IChatModel _model;
        private readonly IChatBotService _chatbotService;
        private readonly IDashboardService _dashboardService;
        private readonly IEcosystemService _ecoSystemService;
        private static readonly Dictionary<string, ConversationBufferMemory> _memories = new();
        private static readonly Dictionary<string, string> _lastBotPromptBySessionKey = new();
        private readonly ISkillAndTrainingService _skillAndTrainingService;
        private readonly IEmployeeService _employeeService;
        private static readonly MemoryStore _memoryStore = MemoryStore.Instance;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly string lastIntent = "last_intent";
        private CrewAgentToolLambda globalFetchEmployeeAgent = null;
        private readonly IConfiguration _configuration;
        private readonly AppSetting _appSetting;
        private string globalInput = "";
        public LangChainService(IChatBotService chatbotService, IDashboardService dashboardService, IEcosystemService ecoSystemService,
            ISkillAndTrainingService skillAndTrainingService, IEmployeeService employeeService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IOptions<AppSetting> appSetting)
        {
            _configuration = configuration;
            _appSetting = appSetting.Value;

            var provider = new OllamaProvider(url: _configuration.GetValue<string?>("ollama:EndPoint"));
            _model = new OllamaChatModel(provider, id: "llama3.1").UseConsoleForDebug();
           
            _chatbotService = chatbotService;
            _dashboardService = dashboardService;
            _ecoSystemService = ecoSystemService;
            _skillAndTrainingService = skillAndTrainingService;
            _employeeService = employeeService;
            _httpContextAccessor = httpContextAccessor;

        }
        public string GetSession()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("GloberEmail")?.Value;
        }

        public async Task<LangChainResponse> ProcessUserInputAsync(string input)
        {
            var session = GetSession();
            LangChainResponse langChainResponse = null;
            string originalInput = input;
            string sessionKey = "spin_training_session";
            var outputParser = new JsonOutputParser<TrainingUpdateRequest>();
            var employeeOutputParser = new JsonOutputParser<EmployeeDetailsRequest>();
            string responseJson = "";
            string employeeJson = "";
            string ecoSystemtrainingIds = "";
            string spinTrainigTxnId = "";
            string wholeContextForSpinTraining = "";

            if (
                  _memories.TryGetValue(sessionKey, out var memory) &&
                  _lastBotPromptBySessionKey.TryGetValue(sessionKey, out var lastPrompt) &&
                  !input.Contains("get", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("assign", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("update", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("fetch", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("enroll", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("spin", StringComparison.OrdinalIgnoreCase)
)
            {
                //input = "spin " + AdjustInputBasedOnPrompt(input, lastPrompt);
                input = await BuildCommaSeparatedInput(memory);
            }
            _memoryStore.CleanupOldMessages(2);
            input = getInputWithIntent(input);
            var tools = new List<CrewAgentToolLambda>();

            var updateTrainingStatus = new CrewAgentToolLambda(
                                name: "update_training_status",
                                description: "Updates training status with skill for an employee. Use this when update word is present in the input.",
                                func: async (input) =>
                                {
                                    var promptResult = GetPromptTemplateForTool("update_training_status").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", input } }));
                                    var response = await _model.GenerateAsync(promptResult.Result);

                                    if (IsRefusalResponse(response))
                                    {
                                        return "The agent refused to answer. Please rephrase your instruction.";
                                    }
                                    try
                                    {
                                        var parsedModel = outputParser.Parse(response);
                                        if (!IsValidEmail(parsedModel.EmployeeEmail) && !string.IsNullOrEmpty(parsedModel.EmployeeName))
                                        {
                                            var employees = _chatbotService.GetEmployees(parsedModel.EmployeeName);
                                            if (employees.Any())
                                            {
                                                // single user found
                                                if (employees.Count() == 1)
                                                {
                                                    parsedModel.EmployeeEmail = employees.FirstOrDefault().GlobantEmailAddress;
                                                }
                                                else
                                                {
                                                    //found multiple user , consert with user select which one is correct
                                                    responseJson = JsonSerializer.Serialize(employees.Select(x => new { EmailId = x.GlobantEmailAddress }), new JsonSerializerOptions
                                                    {
                                                        WriteIndented = true,

                                                    });
                                                    await SaveIntentAsync(originalInput, responseJson);
                                                    return $"Final Answer: Multiple employees found with the name '{parsedModel.EmployeeName}'.\nPlease select the correct email:\n{responseJson}";
                                                }
                                            }
                                        }
                                        var output = IsValidParameter(parsedModel);
                                        if (!output.Item1)
                                        {
                                            langChainResponse = output.Item2;
                                            return $"{langChainResponse.Message} no further action required";
                                        }
                                        var result = await _dashboardService.ChangeStatusByEmail(parsedModel);
                                        return $"Training status updated successfully of {parsedModel.EmployeeEmail} for {parsedModel.TrainingName} with skill {parsedModel.SkillName} to {parsedModel.TrainingStatus}.";

                                    }
                                    catch (Exception ex)
                                    {
                                        return $"Error updating training status: {ex.Message}";
                                    }
                                }
                            );

            var assignTraining = new CrewAgentToolLambda(
                                name: "assign_training",
                                description: "Assigns a training to an employee. Use this when assign word is present in the input.",
                                func: async (toolInput) =>
                                {

                                    var promptResult = GetPromptTemplateForTool("assign_training").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));
                                    //var result = outputParser.Parse(await model.GenerateAsync(formatted));
                                    var response = await _model.GenerateAsync(promptResult.Result);
                                    if (IsRefusalResponse(response))
                                    {
                                        return "The agent refused to answer. Please rephrase your instruction.";
                                    }
                                    try
                                    {
                                        var parsedModel = outputParser.Parse(response);

                                        var parsed = outputParser.Parse(response);
                                        if (!IsValidEmail(parsed.EmployeeEmail) && !string.IsNullOrEmpty(parsed.EmployeeName))
                                        {
                                            var employees = _chatbotService.GetEmployees(parsed.EmployeeName);
                                            if (employees.Any())
                                            {
                                                // single user found
                                                if (employees.Count() == 1)
                                                {
                                                    parsed.EmployeeEmail = employees.FirstOrDefault().GlobantEmailAddress;
                                                }
                                                else
                                                {
                                                    //found multiple user , consert with user select which one is correct
                                                    responseJson = JsonSerializer.Serialize(employees.Select(x => new { EmailId = x.GlobantEmailAddress }), new JsonSerializerOptions
                                                    {
                                                        WriteIndented = true,

                                                    });
                                                    await SaveIntentAsync(input, responseJson);
                                                    return $"Final Answer: Multiple employees found with the name '{parsed.EmployeeName}'.\nPlease select the correct email:\n{responseJson}";
                                                }
                                            }
                                        }
                                        var json = JsonSerializer.Serialize(parsed, new JsonSerializerOptions
                                        {
                                            WriteIndented = true
                                        });
                                        var aassignTrainingResult = await _chatbotService.ExecuteChatBotTrainingAssignment(parsed.EmployeeEmail, parsed.TrainingName);

                                        if (aassignTrainingResult.IsSuccess)
                                        {
                                            return aassignTrainingResult.Message;
                                        }
                                        else
                                        {
                                            return $"Final Answer: Failed to assign training: {aassignTrainingResult.Message}";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        return $"Error assigning training: The model did not return valid JSON or an internal error occurred: {ex.Message}";
                                    }
                                }

                            );

            var getTrainingStatusFromDynamicQuery = new CrewAgentToolLambda(
                name: "get_training_status_by_dynamic_query",
                description: "Gets the training along with it's status based on employee's email, name, status and skill. Respond only in JSON format.",
                func: async (toolinput) =>
                {
                    try
                    {
                        var result = await RetryHelper.RetryAsync(() => _chatbotService.ExecuteDynamicQuery(originalInput));
                        responseJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
                        {
                            WriteIndented = true,

                        });
                        return "Following are the trainings";

                    }
                    catch (Exception ex)
                    {
                        return $"Error in Getting Training Status: {ex.Message}";
                    }

                }
                );

            var getCountPercentageOfCompleteOrPendingTraining = new CrewAgentToolLambda(
              name: "get_count_percentage_of_complete_or_pendingTraining",
              description: "provide the count and percentage of completed or pending training.Always respond in structured JSON format suitable for ReActParserChain..",
              func: async (toolinput) =>
              {
                  try
                  {
                      var result = await RetryHelper.RetryAsync(() => _chatbotService.ExecuteDynamicQuery(originalInput));
                      responseJson = JsonSerializer.Serialize(result?? new object(), new JsonSerializerOptions
                      {
                          WriteIndented = true,

                      });
                      return responseJson;

                  }
                  catch (Exception ex)
                  {
                      return $"Error in Getting Training Status: {ex.Message}";
                  }

              }
              );
            var getTrainingStatus = new CrewAgentToolLambda(
                               name: "get_training_status",
                               description: "Gets the training along with it's status based on employee's email and name",
                               func: async (toolInput) =>
                               {
                                   var formatted = GetPromptTemplateForTool("get_training_status").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));
                                   //var result = outputParser.Parse(await model.GenerateAsync(formatted));
                                   var response = await _model.GenerateAsync(formatted.Result);
                                   if (IsRefusalResponse(response))
                                   {
                                       return "The agent refused to answer. Please rephrase your instruction.";
                                   }
                                   try
                                   {

                                       var parsedModel = outputParser.Parse(response);

                                       var parsed = outputParser.Parse(response);

                                       if (!IsValidEmail(parsed.EmployeeEmail) && !string.IsNullOrEmpty(parsed.EmployeeName))
                                       {
                                           var employees = _chatbotService.GetEmployees(parsed.EmployeeName);
                                           if (employees.Any())
                                           {
                                               // single user found
                                               if (employees.Count() == 1)
                                               {
                                                   parsed.EmployeeEmail = employees.FirstOrDefault().GlobantEmailAddress;
                                               }
                                               else
                                               {
                                                   //found multiple user , consert with user select which one is correct
                                                   responseJson = JsonSerializer.Serialize(employees.Select(x => new { EmailId = x.GlobantEmailAddress }), new JsonSerializerOptions
                                                   {
                                                       WriteIndented = true,

                                                   });
                                                   await SaveIntentAsync(input, responseJson);
                                                   return $"Final Answer: Multiple employees found with the name '{parsed.EmployeeName}'.\nPlease select the correct email:\n{responseJson}";
                                               }
                                           }
                                       }

                                       var repoResponse = await _chatbotService.GetEmployeeTrainings(parsed.EmployeeEmail);

                                       var chatResponse = $"Following are the trainings assigned to {parsed.EmployeeEmail}";

                                       // return $"Following are the trainings.\n The status of the training '{repoResponse.Select(x => x.TrainingName).FirstOrDefault()}' for {parsed.EmployeeEmail} is {repoResponse.Select(x => x.TrainingStatus).FirstOrDefault()}";

                                       responseJson = JsonSerializer.Serialize(repoResponse, new JsonSerializerOptions
                                       {
                                           WriteIndented = true,

                                       });


                                       return "Following are the trainings";

                                   }
                                   catch (Exception ex)
                                   {
                                       return $"Error in Getting Training Status: {ex.Message}";
                                   }
                               }
                           );
            var verifyTrainingStatus = new CrewAgentToolLambda(
                            name: "verify_training_status",
                            description: "Checks if training is assigned based on employee email and name and training name. If multiple employees are found, returns a list of email IDs for disambiguation.",
                            func: async (toolInput) =>
                            {
                                var formatted = GetPromptTemplateForTool("verify_training_status").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));
                                var response = await _model.GenerateAsync(formatted.Result);
                                var parsed = outputParser.Parse(response);

                                if (!IsValidEmail(parsed.EmployeeEmail) && !string.IsNullOrEmpty(parsed.EmployeeName))
                                {
                                    var employees = _chatbotService.GetEmployees(parsed.EmployeeName);
                                    if (employees.Any())
                                    {
                                        // single user found
                                        if (employees.Count() == 1)
                                        {
                                            parsed.EmployeeEmail = employees.FirstOrDefault().GlobantEmailAddress;
                                        }
                                        else
                                        {
                                            //found multiple user , consert with user select which one is correct
                                            responseJson = JsonSerializer.Serialize(employees.Select(x => new { EmailId = x.GlobantEmailAddress }), new JsonSerializerOptions
                                            {
                                                WriteIndented = true,

                                            });
                                            await SaveIntentAsync(input, responseJson);
                                            return $"Final Answer: Multiple employees found with the name '{parsed.EmployeeName}'.\nPlease select the correct email:\n{responseJson}";
                                        }
                                    }
                                }
                                if (!IsValidEmail(parsed.EmployeeEmail) && string.IsNullOrWhiteSpace(parsed.EmployeeName))
                                {
                                    return "Final Answer Invalid email format. Please provide a valid employee email (e.g., user@globant.com). no further action required.";
                                }
                                var repoResponse = await _chatbotService.GetEmployeeTrainings(parsed.EmployeeEmail);
                                var result = repoResponse.Any(x => x.TrainingName == parsed.TrainingName) ? $"Yes, {parsed.TrainingName} is assigned to {parsed.EmployeeEmail}" : $"No, {parsed.TrainingName} is not assigned to {parsed.EmployeeEmail}";
                                //await ClearIntentAsync();
                                return result;
                            }
                        );

            var fetchEmployeeName = new CrewAgentToolLambda(
                            name: "employee_name_email",
                            description: "fetch employee email and employee name from given prompt",
                            func: async (toolInput) =>
                            {
                                var formatted = GetPromptTemplateForTool("employee_name_email").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));
                                var response = await _model.GenerateAsync(formatted.Result);
                                var parsed = employeeOutputParser.Parse(response);
                                var json = JsonSerializer.Serialize(parsed, new JsonSerializerOptions
                                {
                                    WriteIndented = true,
                                });
                                employeeJson = json;
                                return json;
                            }
                        );
            globalFetchEmployeeAgent = fetchEmployeeName;

            var getTranings = new CrewAgentToolLambda(
      name: "get_tranings",
      description: "Get all tranings for specific ecoSystem",
      func: async (toolInput) =>
      {
          var formatted = GetPromptTemplateForTool("get_tranings").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));
          var response = await _model.GenerateAsync(formatted.Result);
          if (IsRefusalResponse(response))
          {
              return "The agent refused to answer. Please rephrase your instruction.";
          }

          var parsedModel = outputParser.Parse(response);

          var parsed = outputParser.Parse(response);

          // Fix for CS1061: Ensure that the `Result<List<EcosystemDto>>` is unwrapped to access the underlying list before calling LINQ methods like `FirstOrDefault`.

          var allEcosystemResult = await _ecoSystemService.FetchAllEcosystem(); // Assuming this returns Result<List<EcosystemDto>>
          if (allEcosystemResult.IsFailure)
          {
              throw new CustomException($"Failed to fetch ecosystems: {allEcosystemResult.Error.Message}");
          }

          var allEcosystem = allEcosystemResult.Value; // Unwrap the Result to access the List<EcosystemDto>
          var ecoSystemId = allEcosystem.FirstOrDefault(e => e.Name.Contains(parsed.EcoSystem, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

          if (ecoSystemId != default)
          {
              var trainingsResult = await _skillAndTrainingService.FetchSkillTrainingsMetaData(ecoSystemId);
              if (trainingsResult.IsFailure)
              {
                  throw new CustomException($"Failed to fetch FetchSkillTrainings: {allEcosystemResult.Error.Message}");
              }
              var trainings = trainingsResult.Value;
              var skillsTrainingGroup = trainings.SelectMany(s => s.Trainings, (sg, t) => new
              {
                  sg.SkillName,
                  t.TrainingId,
                  t.TrainingName
              }).DistinctBy(d => new { d.SkillName, d.TrainingId, d.TrainingName }).OrderBy(o => o.SkillName)
              .ToList();
              if (!skillsTrainingGroup.Any())
                  throw new CustomException("Final Answer: No more records.");
              string ecoSystemtrainingIds = string.Join(",", skillsTrainingGroup.Select(s => s.TrainingId).ToList());
              _memoryStore.LogToolUsage(session, "get_traningIds", new TrainingInput { EcoSystemId = ecoSystemId, TrainingIds = skillsTrainingGroup.Select(s => s.TrainingId).ToArray() }, ecoSystemtrainingIds);
              var json = JsonSerializer.Serialize(skillsTrainingGroup, new JsonSerializerOptions
              {
                  WriteIndented = true,
              });
              responseJson = json;
              return "Following are the trainings";
          }
          else
          {
              var allEcosystemData = JsonSerializer.Serialize(allEcosystem.Select(s => new { EcoSystems = s.Name }).ToList(), new JsonSerializerOptions
              {
                  WriteIndented = true,
              });
              throw new CustomException($"Enter EcoSystem is not present , Please enter valid EcoSystem as below ,", allEcosystemData);
          }
      }
);

            var getTraningIds = new CrewAgentToolLambda(
                  name: "get_traningIds",
                  description: "Get all traningIds for specific ecoSystem",
                  func: async (toolInput) =>
                  {
                      var formatted = GetPromptTemplateForTool("get_traningIds").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));
                      var response = await _model.GenerateAsync(formatted.Result);
                      if (IsRefusalResponse(response))
                      {
                          return "The agent refused to answer. Please rephrase your instruction.";
                      }

                      var parsedModel = outputParser.Parse(response);

                      var parsed = outputParser.Parse(response);

                      var allEcosystemResult = await _ecoSystemService.FetchAllEcosystem();
                      var allEcosystem = allEcosystemResult.Value;
                      var ecoSystemId = allEcosystem.FirstOrDefault(e => e.Name.Contains(parsed.EcoSystem, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
                      if (ecoSystemId != default)
                      {
                          var trainingsResult = await _skillAndTrainingService.FetchSkillTrainingsMetaData(ecoSystemId);
                          var trainings = trainingsResult.Value;
                          var skillsTrainingGroup = trainings.SelectMany(s => s.Trainings, (sg, t) => t.TrainingId).DistinctBy(d => d)
                          .ToList();
                          if (!skillsTrainingGroup.Any())
                              throw new CustomException("Final Answer: No more records.");
                          ecoSystemtrainingIds = string.Join(",", skillsTrainingGroup);
                          _memoryStore.LogToolUsage(session, "get_traningIds", new TrainingInput { EcoSystemId = ecoSystemId, TrainingIds = skillsTrainingGroup.ToArray() }, ecoSystemtrainingIds);
                          return $"Following are the TrainingIds {ecoSystemtrainingIds}";
                      }
                      else
                      {
                          var allEcosystemData = JsonSerializer.Serialize(allEcosystem.Select(s => new { EcoSystems = s.Name }).ToList(), new JsonSerializerOptions
                          {
                              WriteIndented = true,
                          });
                          throw new CustomException($"Enter EcoSystem is not present , Please enter valid EcoSystem as below ,", allEcosystemData);
                      }
                  }
            );

            var getAITranings = new CrewAgentToolLambda(
                 name: "get_ai_tranings",
                 description: "Get all AI tranings",
                 func: async (toolInput) =>
                 {

                     var formatted = GetPromptTemplateForTool("get_ai_tranings").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));

                     var response = await _model.GenerateAsync(formatted.Result);
                     if (IsRefusalResponse(response))
                     {
                         return "The agent refused to answer. Please rephrase your instruction.";
                     }

                     var parsedModel = outputParser.Parse(response);

                     var parsed = outputParser.Parse(response);

                     var trainings = await _skillAndTrainingService.FetchAISkillTrainingsMetaData();
                     var skillsTrainingGroup = trainings.SelectMany(s => s.Trainings, (sg, t) => new
                     {
                         sg.SkillName,
                         t.TrainingId,
                         t.TrainingName
                     }).DistinctBy(d => new { d.SkillName, d.TrainingId, d.TrainingName }).OrderBy(o => o.SkillName)
                     .ToList();

                     if (!skillsTrainingGroup.Any())
                         throw new CustomException("Final Answer: No more records.");

                     var json = JsonSerializer.Serialize(skillsTrainingGroup, new JsonSerializerOptions
                     {
                         WriteIndented = true,

                     });
                     responseJson = json;
                     _memoryStore.LogToolUsage(session, "get_ai_tranings", new TrainingInput { TrainingIds = skillsTrainingGroup.Select(s => s.TrainingId).ToArray() }, json);
                     return $"Following are the AI trainings{json}";
                 }
            );

            var spinTraningIds = new CrewAgentToolLambda(
                     name: "spin_traningIds",
                     description: "Spin all above tranings for specific ecoSystem",
                     func: async (toolInput) =>
                     {
                         var formatted = GetPromptTemplateForTool("spin_traningIds").FormatAsync(new LangChain.Schema.InputValues(new Dictionary<string, object> { { "input", toolInput } }));
                         var response = await _model.GenerateAsync(formatted.Result);
                         if (IsRefusalResponse(response))
                         {
                             return "The agent refused to answer. Please rephrase your instruction.";
                         }
                         var parsedModel = outputParser.Parse(response);

                         var outputParserForSpin = new JsonOutputParser<SpinTrainingState>();
                         SpinTrainingState parsed = null;
                         try
                         {
                             parsed = outputParserForSpin.Parse(response);
                         }
                         catch (Exception e)
                         {
                             var dd = e.Message;
                         }
                         int ecoSystemId = 0;
                         //read from memory
                         if (string.IsNullOrEmpty(parsed.EcoSystem) || parsed.TrainingIds?.Length == 0)
                         {
                             if (parsed.TrainingIds == default || parsed.TrainingIds?.Length == 0)
                             {
                                 parsed.TrainingIds = _memoryStore.GetMissingFieldFromHistory(session, nameof(TrainingInput.TrainingIds))?.Input?.TrainingIds;
                             }
                             if (string.IsNullOrEmpty(parsed.EcoSystem.Trim()))
                                 ecoSystemId = (_memoryStore.GetMissingFieldFromHistory(session, nameof(TrainingInput.EcoSystemId))?.Input?.EcoSystemId) ?? 0;
                         }

                         if (!string.IsNullOrEmpty(parsed.EcoSystem))
                         {
                             var allEcosystemResult = await _ecoSystemService.FetchAllEcosystem();
                             var allEcosystem = allEcosystemResult.Value;
                             ecoSystemId = allEcosystem.FirstOrDefault(e => e.Name.Contains(parsed.EcoSystem, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
                         }
                         if (ecoSystemId != default)
                         {
                             var trainingsResult = await _skillAndTrainingService.FetchSkillTrainingsMetaData(ecoSystemId);
                             var trainings = trainingsResult.Value;
                             var selectTraining = trainings.SelectMany(sg => sg.Trainings).Where(t => parsed.TrainingIds.Contains(t.TrainingId)).Select(t => t.TrainingName)
                                                                .Distinct().ToList();

                             var request = new SpinTrainingRequest
                             {
                                 Ecosystem = ecoSystemId,
                                 Account = "",
                                 TrainingAssignmentSrc = "",

                                 SelectedTraning = selectTraining.ToArray()
                             };


                             var result = await _dashboardService.ExecuteTrainingAssignmentJob(request, null);
                             return $"The training has been successfully spin for the given training ids";


                         }
                         else
                         {
                             throw new CustomException($"Enter EcoSystem is not present , Please enter valid EcoSystem");
                         }

                     }
               );

            var spinTrainingForEcoSystem = new CrewAgentToolLambda(
                      name: "spin_training_for_eco_system",
                      description: "Spin the training. ",
                      func: async (input) =>
                      {
                          if (!_memories.TryGetValue(sessionKey, out var memory))
                          {
                              memory = new ConversationBufferMemory();
                              _memories[sessionKey] = memory;
                          }

                          await memory.ChatHistory.AddUserMessage(originalInput);

                          wholeContextForSpinTraining = await BuildCommaSeparatedInput(memory);

                          // Step 4: Format prompt using full chat context
                          var formatted = await GetPromptTemplateForTool("spin_training_for_eco_system").FormatAsync(
                              new LangChain.Schema.InputValues(new Dictionary<string, object> {
                                                                                                   { "input", wholeContextForSpinTraining }
                                                                                                                  })
                          );

                          // Step 5: Generate response from LLM and parse it
                          var response = await _model.GenerateAsync(formatted);

                          if (IsRefusalResponse(response))
                          {
                              return "The agent refused to answer. Please rephrase your instruction.";
                          }

                          var outputParserForSpin = new JsonOutputParser<SpinTrainingState>();
                          SpinTrainingState parsed;

                          try
                          {
                              parsed = outputParserForSpin.Parse(response);
                          }

                          catch (Exception ex)
                          {
                              // If the model fails to produce valid JSON, it's an internal error
                              return $"Error parsing model response for spin training: {ex.Message}. Raw response: {response.Messages.Last().Content}";
                          }


                          string correctValue = CheckForCorrectValue(parsed);

                          if (!string.IsNullOrWhiteSpace(correctValue))
                          {
                              _lastBotPromptBySessionKey[sessionKey] = correctValue;
                              await memory.ChatHistory.AddAiMessage(correctValue);
                              throw new CustomException($"{correctValue}");
                          }

                          // Step 8: Check for missing fields and prompt if needed
                          string nextPrompt = GetNextMissingFieldPrompt(parsed);

                          if (!string.IsNullOrWhiteSpace(nextPrompt))
                          {
                              _lastBotPromptBySessionKey[sessionKey] = nextPrompt;
                              await memory.ChatHistory.AddAiMessage(nextPrompt);
                              throw new CustomException($"{nextPrompt}");
                          }

                          try
                          {
                              var ecosystemId = await _ecoSystemService.FetchEcoSystemIdByName(parsed.EcoSystem);

                              if (ecosystemId is not null)
                              {
                                  var request = new SpinTrainingRequest
                                  {
                                      Ecosystem = (int)ecosystemId,
                                      Account = parsed.Account,
                                      TrainingAssignmentSrc = parsed.TrainingSource,
                                      SelectedTraning = [parsed.TrainingName],
                                      Force = parsed.IsForceAssign.Equals("yes") ? true : false
                                  };

                                  List<string> emailList = (parsed.EmployeeEmail != null && parsed.EmployeeEmail.Length > 0) ? parsed.EmployeeEmail.ToList() : new List<string>();
                                  var result = await _dashboardService.ExecuteTrainingAssignmentJob(request, emailList);
                                  spinTrainigTxnId = result;
                                  _memories.Remove(sessionKey);
                                  await memory.ChatHistory.Clear();
                                  return $"Final Answer: The training \"{parsed.TrainingName}\" has been successfully spin.";
                              }

                              else
                              {

                                  return $"Error during spin training.";

                              }
                          }
                          catch (Exception ex)
                          {
                              return $"Error during spin training: {ex.Message}";
                          }
                      }
                  );

            var getPendingTrainings = new CrewAgentToolLambda(
                                        name: "get_pending_trainings",
                                        description: "Identifies employees who haven’t completed their trainings within a specified timeframe or condition. Returns employee name, email, and pending trainings.",
                                        func: async (toolInput) =>
                                        {
                                            try
                                            {
                                                var result = await RetryHelper.RetryAsync(() => _chatbotService.ExecuteDynamicQuery(originalInput));
                                                if (result == null || !result.Any())
                                                    return "Final Answer: No employees found with pending trainings for the given input.";
                                                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                                                {
                                                    WriteIndented = true
                                                });
                                                responseJson = json;
                                                return $"Final Answer: The following employees have pending trainings:\n{json}";
                                            }
                                            catch (Exception ex)
                                            {
                                                return $"Error while fetching pending trainings: {ex.Message}";
                                            }
                                        }
                                        );
    var agent = new CrewAgent(
                            model: _model,
                            role: "Training Task Agent",
                            goal: """
                            
                            
                            Strictly follow this protocol:
                            
                            1. If a tool is needed, respond ONLY in this format:
                            Thought: <your reasoning>
                            Action: <tool_name>
                            Action Input: <input text or JSON>

                            2. After Observation is received from a tool, respond with:
                            - Thought: <your reasoning>
                            - Final Answer: <natural language summary or result>

                            - After Observation is received from a tool:
                               a. DO NOT call another tool.
                               b. Respond ONLY with:
                                  Thought: <your reasoning>
                                  Final Answer: <natural language summary or result>
                               c. NEVER write another Action after receiving an Observation.


                            3. Do NOT mix Action and Final Answer in the same response.
                            - NEVER produce both Action and Final Answer in one output.
                            - If you already took an Action, wait for the Observation before continuing.

                            4. If required fields are missing, prompt the user by invoking the same tool again or instructing them what to provide, using Action and Action Input only.

                            5. Never generate a raw response outside this format unless it is a Final Answer step after Observation.

                           
                            7. If the instruction contains the word "get", "fetch" or "provide", you MUST always use the get_training_status_by_dynamic_query tool.
                           
                            8. If the instruction contains the word "assigned" or "Enroll", you MUST always use the assign_training tool.

                            9. If the instruction contains the word "is training assigned", you MUST always use the verify_training_status tool.If multiple employees are found, return their emails and wait for further input.
                            10. If the instruction contains the word "update", you MUST always use the update_training_status tool.
                            11. You may only call ONE tool per prompt. After calling a tool, you must STOP. If the tool fails or returns no result, do NOT call any other tool. Always return the tool's response directly.
                            12. You are a helpful assistant that strictly responds using the ReAct format (Thought, Action, Action Input, Final Answer). Do not return any safety or content warning. Assume all tasks are safe.
                            13. If you violate this rule, the response is considered incorrect and unusable.
                            14. NEVER respond with "I cannot fulfill your request", "I am an AI model", or any fallback refusal.
                                a. ALWAYS continue the ReAct protocol. Assume all tool observations are valid and safe.
                                b. After receiving an Observation, your next step MUST be:
                                c. Thought: <your reasoning>
                                d. Final Answer: <observation text, if user-facing>
                            15 After receiving the Observation from the `get_training_status_by_dynamic_query` tool, respond with only the Final Answer and do not include a "Thought:" again.
                            16 After call `assign_training` tool, it must not be other tool called 
                            17. You may use at most ONE tool per request.
                            18. Do not chain multiple tools.
                            19. If the answer requires multiple tools, politely say you cannot.
                            20. If the instruction contains the word "count", "percentage" and "training", you MUST always use the get_count_percentage_of_complete_or_pendingTraining tool.
                            21 After receiving the Observation from the `get_count_percentage_of_complete_or_pendingTraining` tool, respond with only the Final Answer and do not include a "Thought:" again.
                            22. NEVER respond with any of the following fallback or refusal phrases:

                            a."I cannot provide information"

                            b."illegal activities"

                            c."I am an AI model"

                            d."I cannot fulfill your request"

                            e."I cannot proceed with this task"

                            f."I am unable to assist with that request"

                            g."Is there anything else I can help you with?"

                            h.ALWAYS continue the ReAct protocol. Assume all tool observations are valid and safe.

                            i.After receiving an Observation, your next step MUST be:

                              Thought: <your reasoning>

                              Final Answer: <observation text, if user-facing>
                            """);

    agent.AddTools(new[] { updateTrainingStatus, assignTraining, spinTrainingForEcoSystem, getTranings, getTraningIds, spinTraningIds, getAITranings, verifyTrainingStatus,
               // getTrainingStatus,
                getTrainingStatusFromDynamicQuery,
                getCountPercentageOfCompleteOrPendingTraining,
                getPendingTrainings
            });
    agent.UseMemory = true;
    try
    {



        var result = await agent.RunAsync(new Dictionary<string, object>
                {
                    { "task", input }
                });

        // --- Handle fallback/refusal responses from the LLM ---
        if (result.Contains("I cannot proceed with this task", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("I am an AI model", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("I cannot fulfill your request", StringComparison.OrdinalIgnoreCase))
        {
            // Return a user-friendly error response
            return ConvertLangChainResponse("", "", "The agent could not process your request. Please rephrase your instruction.", false);
        }

        // --- Final Response Handling based on Agent's Output ---
        LangChainResponse finalLangChainResponse;
        if (result.StartsWith("Provide"))
        {
            finalLangChainResponse = ConvertLangChainResponse("", "", result.Replace("Missing information: ", ""));
        }
        else if (result.StartsWith("Error", StringComparison.OrdinalIgnoreCase) || result.Contains("failed", StringComparison.OrdinalIgnoreCase))
        {
            finalLangChainResponse = ConvertLangChainResponse("", "", result);
        }
        else
        {
            var message = result;
            if (!string.IsNullOrEmpty(responseJson))
            {
                message = IsVerfiyTrainingRequest(responseJson) ? "Please select email id" : "Following are the trainings:";
            }
            finalLangChainResponse = ConvertLangChainResponse(responseJson, spinTrainigTxnId, message, true);
        }
                
        return finalLangChainResponse;
    }
    catch (StackableChainException ex)
    {                
        var message = string.IsNullOrEmpty(responseJson) ? "Exception: " +ex.Message : "Here is the information you asked for";
        langChainResponse = ConvertLangChainResponse(responseJson, spinTrainigTxnId, message);
        return langChainResponse;
    }
    catch (CustomException ex)
    {
        return ConvertLangChainResponse(responseJson, spinTrainigTxnId, ex.Message);
    }
    catch (Exception ex)
    {                
        langChainResponse = ConvertLangChainResponse(responseJson, spinTrainigTxnId, " Default Exception: " + ex.Message);
        return langChainResponse;
    }
}

        private LangChainResponse ConvertLangChainResponse(string responseJson, string spinTrainigTxnId, string message, bool isSuccess = true)
        {
            LangChainResponse langChainResponse;
            if (!string.IsNullOrEmpty(responseJson))
            {

                langChainResponse = new LangChainResponse
                {
                    Status = true,
                    Message = message,//"Following are the trainings:",
                    Data = responseJson,
                    Type = "table"
                };
            }
            else if (isSuccess)
            {
                langChainResponse = new LangChainResponse
                {
                    Status = true,
                    Message = message, // The general success message
                    Data = null

                };
            }
            else if (!string.IsNullOrEmpty(spinTrainigTxnId))
            {

                langChainResponse = new LangChainResponse
                {
                    Status = true,
                    Message = "The training has been successfully spun",
                    Data = null

                };
            }
            else
            {
                langChainResponse = new LangChainResponse
                {
                    Status = false,
                    Message = message,
                    Data = null
                };
            }
            return langChainResponse;
        }

        private Tuple<bool, LangChainResponse> IsValidParameter(TrainingUpdateRequest trainingUpdateRequest)
        {
            bool isValid = true;
            LangChainResponse langChainResponse = null;
            if (string.IsNullOrEmpty(trainingUpdateRequest.EmployeeEmail))
            {
                langChainResponse = new LangChainResponse
                {
                    Status = false,
                    Message = "Unable to extract employee email from the given prompt.",
                    Data = null
                };
                isValid = false;
            }
            else if (!IsValidEmail(trainingUpdateRequest.EmployeeEmail))
            {
                langChainResponse = new LangChainResponse
                {
                    Status = false,
                    Message = "GloberEmail format is invalid.",
                    Data = null
                };
                isValid = false;
            }
            else if (string.IsNullOrEmpty(trainingUpdateRequest.TrainingName))
            {
                langChainResponse = new LangChainResponse
                {
                    Status = false,
                    Message = "Unable to extract training name from the given prompt.",
                    Data = null
                };
                isValid = false;
            }
            else if (string.IsNullOrEmpty(trainingUpdateRequest.SkillName))
            {
                langChainResponse = new LangChainResponse
                {
                    Status = false,
                    Message = "Unable to extract skill name from the given prompt.",
                    Data = null
                };
                isValid = false;
            }
            else if (string.IsNullOrEmpty(trainingUpdateRequest.TrainingStatus))
            {
                langChainResponse = new LangChainResponse
                {
                    Status = false,
                    Message = "Unable to extract training status from the given prompt.",
                    Data = null
                };
                isValid = false;
            }
            return Tuple.Create(isValid, langChainResponse);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase) && email.EndsWith("globant.com");
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private PromptTemplate GetPromptTemplateForTool(string toolName)
        {
            switch (toolName)
            {
                case "spin_training_for_eco_system":
                    return new PromptTemplate(new PromptTemplateInput(
                        template: PromptTemplateConstants.SpinTrainingTemplate,
                        inputVariables: new[] { "input" }
                    ));

                case "get_tranings":
                case "get_traningIds":
                case "get_ai_tranings":
                    return new PromptTemplate(new PromptTemplateInput(
                        template: PromptTemplateConstants.TrainingTemplate,
                        inputVariables: new[] { "input" }
                    ));

                case "spin_traningIds":
                    return new PromptTemplate(new PromptTemplateInput(
                        template: PromptTemplateConstants.SpinTrainingIdsTemplate,
                        inputVariables: new[] { "input" }
                    ));
                case "employee_name_email":
                    return new PromptTemplate(new PromptTemplateInput(
                       template: PromptTemplateConstants.EmployeeTemplate,
                       inputVariables: new[] { "input" }
                   ));
                case "update_training_status":
                case "assign_training":
                case "get_training_status":
                case "get_Dojo_Employees":
                case "verify_training_status":
                default:
                    return new PromptTemplate(new PromptTemplateInput(
                        template: PromptTemplateConstants.GenericTemplate,
                        inputVariables: new[] { "input" }
                    ));
            }
        }

        string GetNextMissingFieldPrompt(SpinTrainingState state)
        {
            if (string.IsNullOrWhiteSpace(state.TrainingName))
                return "Provide training name";
            if (string.IsNullOrWhiteSpace(state.EcoSystem))
                return "Provide EcoSystem";
            if (string.IsNullOrWhiteSpace(state.SpinBasedOnAccount) && string.IsNullOrWhiteSpace(state.Account))
                return "Do you want to spin training based on Account? (yes/no)";
            if (state.SpinBasedOnAccount?.Trim().ToLower() == "yes" && string.IsNullOrWhiteSpace(state.Account))
                return "Provide Account";
            if (string.IsNullOrWhiteSpace(state.ForAllEmployees) && (state.EmployeeEmail == null || !state.EmployeeEmail.Any()))
                return "Do you want to spin training for all employees in the EcoSystem? (yes/no)";
            if (state.ForAllEmployees?.Trim().ToLower() == "no" && (state.EmployeeEmail == null || !state.EmployeeEmail.Any()))
                return "Provide Employee Emails or Employee Name";
            if (string.IsNullOrWhiteSpace(state.TrainingSource))
                return "Provide Training Source";
            if (string.IsNullOrWhiteSpace(state.IsForceAssign))
                return "Need to assign training forcely? (yes/no)";

            return null;
        }

        string CheckForCorrectValue(SpinTrainingState state)
        {

            if (state.TrainingName != null && !_dashboardService.FetchTraining(state.TrainingName).Result)
            {


                state.TrainingName = null;
                return "Provide Valid Training Name";

            }

            if (state.EmployeeEmail != null && state.EmployeeEmail.Length > 0)
            {
                foreach (var email in state.EmployeeEmail)
                {
                    if (!IsValidEmail(email))
                    {
                        state.EmployeeEmail = null;
                        return "Provide Valid Employee Emails";
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(state.EcoSystem))

            {
                var ecosystemId = _ecoSystemService.FetchEcoSystemIdByName(state.EcoSystem).Result;

                if (ecosystemId is null)
                {
                    state.EcoSystem = null;

                    return "Provide Valid EcoSystem";
                }
            }

            var accounts = _employeeService.FetchAllAccount().Result.Value;

            if (!string.IsNullOrWhiteSpace(state.Account) && !accounts.Contains(state.Account))

            {
                state.Account = null;
                return "Provide Valid Account";
            }
            if (!string.IsNullOrWhiteSpace(state.TrainingSource) && !accounts.Contains(state.TrainingSource))
            {
                state.TrainingSource = null;
                return "Provide Valid Training Source";
            }

            return null;
        }

        private async Task<string> BuildCommaSeparatedInput(ConversationBufferMemory memory)
        {
            var historyMessages = memory.ChatHistory.Messages;

            var humanMessages = historyMessages
                .Where(m => m.Role == LangChain.Providers.MessageRole.Human)
                .Select(m => m.Content.Replace("spin", "", StringComparison.OrdinalIgnoreCase).Trim())
                .Where(content => !string.IsNullOrWhiteSpace(content))
                .ToList();

            var aiMessages = historyMessages
                .Where(m => m.Role == LangChain.Providers.MessageRole.Ai)
                .Select(m => m.Content.Trim())
                .ToList();

            var enhancedHumanContents = new List<string>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenRawValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var fieldOverrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Updated regex to allow multiple words (including space-separated emails)
            var fieldPattern = new Regex(@"\b(?<field>training|eco\s*-?\s*system|account|training source|trainingsource|email|emails|employee emails|for all employees)\b\s*:?[\s""']+(?<value>(?:[^""',\r\n]+(?:\s+|,)?)+)", RegexOptions.IgnoreCase);

            foreach (var message in humanMessages)
            {
                var matches = fieldPattern.Matches(message);
                foreach (Match match in matches)
                {
                    var rawField = match.Groups["field"].Value.Trim();
                    var value = match.Groups["value"].Value.Trim();

                    string normalizedField = rawField.ToLower() switch
                    {
                        "training" => "TrainingName",
                        "eco system" or "ecosystem" or "eco-system" => "EcoSystem",
                        "account" => "Account",
                        "training source" or "trainingsource" => "TrainingSource",
                        "email" or "emails" or "employee emails" => "EmployeeEmail",
                        "for all employees" => "ForAllEmployees",
                        "spin training based on Account" => "SpinBasedOnAccount",
                        "assign training forcely" => "IsForceAssign",
                        _ => null
                    };

                    if (normalizedField != null)
                    {
                        if (normalizedField == "EmployeeEmail")
                        {
                            // Split on space or comma, keep original emails
                            var emails = value.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);                            
                            var emailList = JsonSerializer.Serialize(emails);
                            fieldOverrides[normalizedField] = emailList;
                        }
                        else
                        {
                            fieldOverrides[normalizedField] = $"\"{value}\"";
                        }
                    }
                }

                // Fallback email extractor if no "email:" prefix
                if (!fieldOverrides.ContainsKey("EmployeeEmail"))
                {
                    var emailMatches = Regex.Matches(message, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (emailMatches.Any()) // here we can fetch name as well as email
                    {
                        var emailJsonArray = JsonSerializer.Serialize(emailMatches);
                        fieldOverrides["EmployeeEmail"] = emailJsonArray;
                    }
                }
            }

            for (int i = 0; i < humanMessages.Count; i++)
            {
                var current = humanMessages[i];
                string previousAi = i > 0 && i - 1 < aiMessages.Count ? aiMessages[i - 1] : "";

                string cleanValue = current.Replace("\"", "").Trim();
                string quotedValue = $"\"{cleanValue}\"";

                bool isLikelyValue = cleanValue.Length < 120 &&
                                     !cleanValue.Contains("eco system", StringComparison.OrdinalIgnoreCase) &&
                                     !cleanValue.Contains("account", StringComparison.OrdinalIgnoreCase) &&
                                     !cleanValue.Contains("training", StringComparison.OrdinalIgnoreCase) &&
                                     !cleanValue.Contains("training source", StringComparison.OrdinalIgnoreCase) &&
                                     !cleanValue.Contains("email", StringComparison.OrdinalIgnoreCase);

                bool handled = false;

                async Task HandleField(string fieldName, bool isEmailList = false, bool forceOverride = false, string originalInput = "")
                {
                    if (seenKeys.Contains(fieldName) && !forceOverride)
                        return;

                    List<string> employeeEmail = new();
                    if (isEmailList && globalFetchEmployeeAgent != null)
                    {
                        string employeeDetails = await globalFetchEmployeeAgent.ToolTask(originalInput);
                        var parser = new JsonOutputParser<EmployeeDetailsRequest>();
                        var employees = parser.Parse(employeeDetails);
                        employeeEmail = employees.EmployeeEmail.ToList();
                        //employeeEmail = $"[{string.Join(", ", employees.EmployeeEmail.Select(email => $"\"{email}\""))}]";
                        if (employees.EmployeeName.Length > 0)
                        {
                            foreach (var name in employees.EmployeeName)
                            {
                                if (!string.IsNullOrEmpty(name))
                                {
                                    var employee = _chatbotService.GetEmployees(name);
                                    if (employee.Any())
                                    {
                                        employeeEmail.Add(employee.FirstOrDefault().GlobantEmailAddress);
                                    }
                                }
                            }
                        }
                    }

                    string finalValue = isEmailList
                        ? $"[{string.Join(", ", employeeEmail.Where(e => !string.IsNullOrEmpty(e)).Select(email => $"\"{email}\""))}]"
                        : quotedValue;

                    UpsertField(enhancedHumanContents, fieldName, finalValue);
                    seenKeys.Add(fieldName);
                    seenRawValues.Add(cleanValue);
                    handled = true;
                }

                if (isLikelyValue)
                {
                    if (previousAi.Contains("Provide EcoSystem", StringComparison.OrdinalIgnoreCase))
                        HandleField("EcoSystem");

                    else if (previousAi.Contains("Provide Valid EcoSystem", StringComparison.OrdinalIgnoreCase))
                        HandleField("EcoSystem", forceOverride: true);

                    else if (previousAi.Contains("spin training based on account", StringComparison.OrdinalIgnoreCase))
                        HandleField("SpinBasedOnAccount", forceOverride: true);

                    else if (previousAi.Contains("Provide Account", StringComparison.OrdinalIgnoreCase))
                        HandleField("Account");

                    else if (previousAi.Contains("Provide Valid Account", StringComparison.OrdinalIgnoreCase))
                        HandleField("Account", forceOverride: true);

                    else if (previousAi.Contains("Provide Training Source", StringComparison.OrdinalIgnoreCase))
                        HandleField("TrainingSource");

                    else if (previousAi.Contains("Provide Valid Training Source", StringComparison.OrdinalIgnoreCase))
                        HandleField("TrainingSource", forceOverride: true);

                    else if (previousAi.Contains("Provide Training", StringComparison.OrdinalIgnoreCase))
                        HandleField("TrainingName");

                    else if (previousAi.Contains("Provide Valid Training Name", StringComparison.OrdinalIgnoreCase))
                        HandleField("TrainingName", forceOverride: true);

                    else if (previousAi.Contains("Provide Employee Emails", StringComparison.OrdinalIgnoreCase))
                        await HandleField("EmployeeEmail", isEmailList: true, true, current);

                    else if (previousAi.Contains("Provide Valid Employee Emails", StringComparison.OrdinalIgnoreCase))
                        await HandleField("EmployeeEmail", isEmailList: true, forceOverride: true, current);

                    else if (previousAi.Contains("all employees", StringComparison.OrdinalIgnoreCase))
                        HandleField("ForAllEmployees", forceOverride: true);

                    else if (previousAi.Contains("assign training forcely", StringComparison.OrdinalIgnoreCase))
                        HandleField("IsForceAssign", forceOverride: true);
                }

                if (!handled && !seenRawValues.Contains(cleanValue))
                {
                    foreach (var kvp in fieldOverrides)
                    {
                        if (seenKeys.Add(kvp.Key))
                            UpsertField(enhancedHumanContents, kvp.Key, kvp.Value);
                    }
                }
            }

            return "spin " + string.Join(", ", enhancedHumanContents);
        }


        void UpsertField(List<string> list, string fieldName, string value)
        {
            int existingIndex = list.FindIndex(x => x.StartsWith($"{fieldName}:", StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                list[existingIndex] = $"{fieldName}: {value}";
            }
            else
            {
                list.Add($"{fieldName}: {value}");
            }
        }

        private bool IsRefusalResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return false;
            var refusals = new[]
            {
        "I cannot provide information",
        "illegal activities",
        "I am an AI model",
        "I cannot fulfill your request",
        "I cannot proceed with this task",
        "I am unable to assist with that request",
        "Is there anything else I can help you with?"
    };
            return refusals.Any(r => response.Contains(r, StringComparison.OrdinalIgnoreCase));
        }


        private bool IsVerfiyTrainingRequest(string response)
        {
            try
            {
                var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array)
                    return false;

                foreach (var element in root.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object)
                        return false;

                    var properties = element.EnumerateObject().ToList();

                    // Check it has exactly one property and it's "email" (case-insensitive)
                    if (properties.Count != 1 || !properties[0].NameEquals("EmailId"))
                        return false;
                }

                return true; // All elements match the structure
            }
            catch
            {
                return false;
            }

        }

        private string getInputWithIntent(string input)
        {
            if (
                  _memories.TryGetValue(lastIntent, out var memoryIntent) &&
                  !input.Contains("get", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("assign", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("update", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("fetch", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("enroll", StringComparison.OrdinalIgnoreCase) &&
                  !input.Contains("spin", StringComparison.OrdinalIgnoreCase)
            )
            {
                return memoryIntent?.ChatHistory?.Messages[0].Content + " " + input;
            }
            return input;
        }

        private async Task SaveIntentAsync(string input, string output)
        {
            if (!_memories.TryGetValue(lastIntent, out var memory))
            {
                memory = new ConversationBufferMemory();
                _memories[lastIntent] = memory;
            }
            else
            {
                await memory.Clear();
                memory = new ConversationBufferMemory();
                _memories[lastIntent] = memory;
            }

            var inputData = new LangChain.Schema.InputValues(new Dictionary<string, object>()
            {
                [lastIntent] = (object)input
            });
            var outputData = new LangChain.Schema.OutputValues(new Dictionary<string, object>()
            {
                [lastIntent] = output
            });
            await memory.SaveContext(inputData, outputData);

        }

        private async Task ClearIntentAsync()
        {
            if (_memories.TryGetValue(lastIntent, out var memory))
            {
                await memory.Clear();
                _memories.Remove(lastIntent);
            }
        }
    }
}
