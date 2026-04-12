using Academy.Core.Abstraction.Services;
using Academy.Shared.DTO;
using Academy.Shared.DTO.DBSchema;
using Academy.Shared.Enums;
using Academy.Shared.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
namespace Academy.Core.Services
{
    public class AIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly AppSetting _appSetting;
        private IChatClient aiClient;
        public AIService(IConfiguration configuration, IOptions<AppSetting> appSetting)
        {
            _configuration = configuration;
            _appSetting = appSetting.Value;
        }


        public async Task<AIQuery> GetAISQLQuery(string aiModel, AIServices aiService, string userPrompt, DatabaseSchema dbSchema, string databaseType)
        {           
            var ollamaEndpoint = _configuration.GetValue<string?>("ollama:EndPoint");
            aiClient = CreateChatClient(aiModel, aiService);
            List<ChatMessage> chatHistory = new List<ChatMessage>();
            var builder = GenerateContent(dbSchema, userPrompt);


            // Build the AI chat/prompts
            if (string.IsNullOrEmpty(ollamaEndpoint))
            {
                // Ollama doesn't play well with system prompts and large context windows, so the main prompt can't be a system prompt when Ollama is enabled
                // This also means we have to disable supplemental chat tab :(
                chatHistory.Add(new ChatMessage(ChatRole.System, builder.ToString()));
            }
            else
            {
                chatHistory.Add(new ChatMessage(ChatRole.User, builder.ToString()));
            }

            chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

            // Send request to Azure OpenAI model
            var options = new ChatOptions
            {
                Temperature = 0.0f // Set deterministic output
            };
            var response = await aiClient.GetResponseAsync(chatHistory, options);
            var responseContent = response.Messages[0].Text.Replace("```json", "").Replace("```", "").Replace("\\n", " ");

            try
            {
                return JsonSerializer.Deserialize<AIQuery>(responseContent);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parAI response as a SQL Query. The AI response was: " + response.Messages[0].Text);
            }
            throw new NotImplementedException();
        }
        private IChatClient CreateChatClient(string aiModel, AIServices aiService)
        {
            switch (aiService)
            {
                case AIServices.Ollama:                    
                    var ollamaEndpoint = _configuration.GetValue<string?>("ollama:EndPoint");
                    return new OllamaSharp.OllamaApiClient(ollamaEndpoint, aiModel);
                case AIServices.OpenAI:
                    OpenAIClientOptions openAIClientOptions = new OpenAIClientOptions() { Endpoint = new Uri(_appSetting.OpenAISettings.EndPoint) };
                    OpenAIClient client = new OpenAIClient(new ApiKeyCredential(_appSetting.OpenAISettings.Rag_AI_Token.Decrypt()), openAIClientOptions);
                    return new OpenAIChatClient(client, aiModel);
            }

            return null;
        }

        /// <summary>
        /// Generate Content by adding table, relationship and instructions
        /// </summary>
        /// <param name="dbSchema"></param>
        /// <returns></returns>
        private string GenerateContent(DatabaseSchema dbSchema, string userPrompt)
        {
            var result = new StringBuilder();
            result.AppendLine(@"Your are a helpful database assistant. Do not respond with any information unrelated to databases or queries. 
                                Use the following database schema and relationships when creating your answers:");
            AddTable(dbSchema, result);
            AddRelationShip(result);
            AddRule(result, "", userPrompt);

            return result.ToString();
        }

        /// <summary>
        /// Add tables 
        /// </summary>
        /// <param name="dbSchema"></param>
        /// <param name="builder"></param>
        private void AddTable(DatabaseSchema dbSchema, StringBuilder builder)
        {
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table);
            }
        }

        /// <summary>
        /// Add all relationship between tables
        /// </summary>
        /// <param name="builder"></param>
        private void AddRelationShip(StringBuilder builder)
        {

            builder.AppendLine(@"Relationships:
                                    - EmployeeTrainingMap.EmployeeId → Employee.Id
                                    - EmployeeTrainingMap.TrainingId → TrainingMaster.TrainingId
                                    - EmployeeTrainingMap.TrainingStatusId → TrainingStatusMaster.TrainingStatusId
                                    - EmployeeTrainingMap.SkillId → SkillMaster.SkillId
                                    
                                    - EmployeeActivityMap.EmployeeId → Employee.Id
                                    - EmployeeActivityMap.ActivityId → ActivityMaster.ActivityId
                                    
                                    - EmployeeRoleMap.EmployeeId → Employee.Id
                                    - EmployeeRoleMap.RoleId → RoleMaster.RoleId
                                    
                                    - SkillEndorsementMap.EmployeeId → Employee.Id
                                    - SkillEndorsementMap.SkillId → SkillMaster.SkillId
                                    
                                    - SkillMaster.CategoryId → CategoryMaster.CategoryId
                                    
                                    - TrainingProficiencyMap.TrainingId → TrainingMaster.TrainingId
                                    - TrainingProficiencyMap.SkillId → SkillMaster.SkillId
                                    - TrainingProficiencyMap.SeniorityId → SeniorityMaster.SeniorityId
                                    - TrainingProficiencyMap.EcosystemId → EcosystemMaster.EcosystemId
                                    
                                    - Interview.SeniorityId → SeniorityMaster.SeniorityId
                                    - InterviewSkillSet.interview_id → Interview.Id
                                    - InterviewSkillSet.skill_id → SkillMaster.SkillId
                                    
                                    - Candidate_Evaluation.interview_id → Interview.Id
                                    - Candidate_Evaluation.question_id → Question.question_id
                                    
                                    - Question.SkillId → SkillMaster.SkillId
                                    - Question_SeniorityMaster.question_id → Question.question_id
                                    - Question_SeniorityMaster.SeniorityId → SeniorityMaster.SeniorityId");
        }

        private void AddRule(StringBuilder builder, string databaseType, string userPrompt)
        {
            builder.AppendLine("Include column name headers in the query results.");
            builder.AppendLine("Always provide your answer in the JSON format below:");
            //builder.AppendLine("When writing SQL queries, always use 'TOP n' instead of 'LIMIT n'.");
            builder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
            builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-query"" with the database query used to retrieve the requested data.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-summary"" with an explanation of each step you took to create this query in a detailed paragraph.");
            builder.AppendLine($"Only use {databaseType} syntax for database queries.");
            //builder.AppendLine($"Always limit the SQL Query to {1000} rows.");
            builder.AppendLine("Always include all of the table columns and details.");
            builder.AppendLine("Always include all column which are selected in group by clause if it is used.");
            builder.AppendLine("When generating SQL queries, always return human-readable column names (e.g., EmployeeName, DepartmentName, SkillName) instead of ID columns (e.g., EmployeeId, DepartmentId, SkillId).\r\n\r\nUse JOINs to get the name columns from the related tables where applicable.\r\n\r\nAvoid selecting raw ID columns in the final SELECT clause unless specifically requested.\r\n\r\nOnly include names, descriptions, or labels that make the result easily understandable.");

            DetermineIntent(userPrompt, builder);
        }
        private void AddFetchTrainingStatusRule(StringBuilder builder, string userPrompt)
        {
            builder.AppendLine(@"- Join Employee → EmployeeTrainingMap → TrainingMaster → TrainingStatusMaster → SkillMaster
            - Columns in the result (output) and their sources:
                - EmployeeName → from Employee.EmployeeName
                - TrainingName → from TrainingMaster.TrainingName
                - TrainingStatusName → from TrainingStatusMaster.TrainingStatusName
                - SkillName → from SkillMaster.SkillName
            - Use EmployeeTrainingMap.TrainingStatusId → TrainingStatusMaster.TrainingStatusId to get status
            - Use EmployeeTrainingMap.TrainingId → TrainingMaster.TrainingId to get training name
            - Use EmployeeTrainingMap.SkillId → SkillMaster.SkillId to get Skill name
            - Do NOT join Employee directly to  SkillMaster
            - Do NOT use the table TrainingProficiencyMap at all
            - The result should show training status along with the training name for the given employee and/or training
            - Both filters are optional: if one is not provided, the query returns all relevant records");
        }
        private void AddRuleToIdentifyIncompleteTraining(StringBuilder builder)
        {
            builder.AppendLine(@"- Filter for trainings where DATEDIFF(DAY, EmployeeTrainingMap.StartDate, GETDATE()) > 30
                                 - Only include trainings not completed (EmployeeTrainingMap.ActualEndDate IS NULL)
                                 - Return columns: EmployeeName, TrainingName, TrainingStatusName
                                 - Do NOT join Employee directly to TrainingProficiencyMap or SkillMaster");
        }
        private void AddRuleForCalculatingPercentage(StringBuilder builder)
        {
            builder.AppendLine(@"- Join Employee → EmployeeTrainingMap → TrainingStatusMaster
                                 - Use EmployeeTrainingMap.TrainingStatusId → TrainingStatusMaster.TrainingStatusId to get the training status
                                 - Use EmployeeTrainingMap.EmployeeId → Employee.Id to link to Employee
                                 - Columns in the result (output) and their sources:
                                     - EmployeeName → from Employee.EmployeeName
                                     - TotalTraining → total number of trainings for the employee (COUNT of EmployeeTrainingMap.TrainingId)
                                     - CompletedTrainingCount → count of trainings where TrainingStatusMaster.TrainingStatusName = 'Completed'
                                     - PendingTrainingCount → count of trainings where TrainingStatusMaster.TrainingStatusName = 'Pending'
                                     - CompletedPercentage → (CompletedTrainingCount / TotalTraining) * 100; CAST numerator as FLOAT to avoid integer division
                                     - PendingPercentage → (PendingTrainingCount / TotalTraining) * 100; CAST numerator as FLOAT to avoid integer division
                                 - Group results by Employee.EmployeeName
                                 - Order results by Employee.EmployeeName
                                 - Do NOT join Employee directly to TrainingProficiencyMap or SkillMaster
                                 - This instruction calculates total trainings, counts and percentages of completed and pending trainings per employee, with optional filters");

        }
        void DetermineIntent(string prompt, StringBuilder builder)
        {
            prompt = prompt.ToLower();

            if ((prompt.Contains("fetch") || prompt.Contains("get") || prompt.Contains("provide"))
                && prompt.Contains("training")
                && prompt.Contains("status"))
            {
                AddFetchTrainingStatusRule(builder, prompt);
            }

            if ((prompt.Contains("identify") || prompt.Contains("not completed") || prompt.Contains("overdue"))
                && prompt.Contains("training"))
            {
                AddRuleToIdentifyIncompleteTraining(builder);
            }

            if ((prompt.Contains("count") || prompt.Contains("percentage"))
                && prompt.Contains("completed") || prompt.Contains("pending"))
            {
                AddRuleForCalculatingPercentage(builder);
            }
        }
        public static void Retry(Action action, int maxRetries = 3, int delayMilliseconds = 1000)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    action();
                    return; // Success
                }
                catch (Exception ex)
                {                    
                    if (attempt == maxRetries)
                        throw; // Rethrow on last attempt

                    Thread.Sleep(delayMilliseconds);
                }
            }
        }
    }
}
