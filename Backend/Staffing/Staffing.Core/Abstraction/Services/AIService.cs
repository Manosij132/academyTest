using Academy.Shared.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using Staffing.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Services
{
    public class AIService(IConfiguration config, ISemanticKernelService chatClientService)
    {
        public async Task<AIQuery> GetAISQLQuery(string aiModel, string aiService, string userPrompt, DatabaseSchema dbSchema, string databaseType, StructuredAgent structuredAgent = StructuredAgent.None)
        {
            var history = chatClientService.LoadChatHistoryFromDB() ?? new ChatHistory();

            var builder = new StringBuilder();
            var maxRows = config.GetValue<string>("MAX_ROWS");

            builder.AppendLine("Your are a helpful database assistant. Do not respond with any information unrelated to databases or queries. Use the following database schema when creating your answers:");
            builder.AppendLine("Use Microsoft SQL Server sql syntax");
            builder.AppendLine("Let us think Step by Step"); //Zero shot Chain of thought prompting to get high accuracy.
            foreach (var table in dbSchema.SchemaRaw)
            {
                builder.AppendLine(table.Value);
            }
            builder.AppendLine("Consider only given database schema.");
            builder.AppendLine("Include column name headers in the query results.");
            builder.AppendLine("Always provide your answer in the JSON format below:");
            builder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
            builder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-query"" with the database query used to retrieve the requested data.");
            builder.AppendLine(@"In the preceding JSON response, substitute ""your-summary"" with an explanation of each step you took to create this query in a detailed paragraph.");
            builder.AppendLine($"Only use {databaseType} syntax for database queries and validate it before sending back.");
            builder.AppendLine("Always include required table columns.");
            builder.AppendLine("Do not make unwanted self joins.");
           // builder.AppendLine("Do not include unwanted additional Where conditions.");
            builder.AppendLine($"Always consider all the messages from history by {AuthorRole.User}. while generating sql query");
            builder.AppendLine("Include column name headers with AS keyword if you are using aggregate functions.");            
            builder.AppendLine("Always include all column which are selected in group by clause if it is used. Note: You can NEVER include aggregate functions (COUNT, SUM, AVG, MAX, MIN) inside GROUP BY");
            builder.AppendLine("When generating SQL queries, always return human-readable column names (e.g., EmployeeName, DepartmentName, SkillName) instead of ID columns (e.g., EmployeeId, DepartmentId, SkillId).\r\n\r\nUse JOINs to get the name columns from the related tables where applicable.\r\n\r\nAvoid selecting raw ID columns in the final SELECT clause unless specifically requested.\r\n\r\nOnly include names, descriptions, or labels that make the result easily understandable.");
            builder.AppendLine("Important Note: The SQL Query should be syntactically correct");
            
            AgentPrompts.AddPromptsExample(builder, structuredAgent);
            AgentPrompts.PromptRefinement(builder);

            history.AddDistinctSystemMessage(builder.ToString());           
            history.AddDistinctSystemMessage("You are a helpful and memory-aware assistant.");
            history.AddMessage(Microsoft.SemanticKernel.ChatCompletion.AuthorRole.User, userPrompt);
            var response = await chatClientService.GetResponseAsync(history);
            var responseContent = response.ToString().Replace("```json", "").Replace("```", "").Replace("\\n", " ").Replace("\n"," ");

            return JsonSerializer.Deserialize<AIQuery>(responseContent)!;
        }

        public async Task<string> GetSummary(string userprompt, DatabaseSchema schema, ISemanticKernelService _chatClientService)
        {
            var columns = schema.SchemaStructured.Where(s => s.TableName == "[dbo.StaffRequests]").FirstOrDefault().Columns;
            var prompt = $@"
            Generated sql query: {userprompt}
            Here is the list of columns: {System.Text.Json.JsonSerializer.Serialize(columns)}
            Summarize the sql query in the business language taking into account where conditions, sorting and grouping using the columns provided
            Start with Here is the summary
            Include only the summary. Do not include any other information
            Do not have any SQL Query related terms in the summary including query term
            be natural and context-aware — not just technical SQL, but meaningful from a business, with no greeting words and short queries 
            ";
            var queryresult = await _chatClientService.GenerateSuggestedPrompt(prompt, columns);
            return queryresult;

        }


        public async Task<List<SuggestedQuestionsDTO>> GenerateClarifyingQuestions(string userPrompt, DatabaseSchema dbSchema, ISemanticKernelService _chatClientService)
        {

            var columns = dbSchema.SchemaStructured.Where(s => s.TableName == "[dbo.StaffRequests]").FirstOrDefault().Columns;

            var prompt = $@"
            Generated sql query: {userPrompt}
            Here is the list of columns: {System.Text.Json.JsonSerializer.Serialize(columns)}
            Generate exactly 6 clarifying suggestions for columns NOT present in the SQL query.

            Each suggestion MUST be a SINGLE LINE and MUST contain:
            - one affirmative Prompt
            - one Question
            - separated by EXACTLY ONE pipe character (|)
            
            FORMAT (MANDATORY):
            <Prompt> | <Question>
            
            Rules for ALL output lines:
            - Every line MUST include both a Prompt AND a Questionx
            - Lines missing either part are INVALID and must not be generated
            - Do NOT output standalone prompts or standalone questions
            - Do NOT output explanations, headings, or empty lines
            
            Question rules:
            - Must ask whether the user wants to filter data
            - Must be phrased as a question
            - Must be based only on available columns
            - Must NOT reference SQL or column names
            - Must be business-friendly and context-aware
            - Must NOT repeat the same column intent
            - Must NOT include columns already in the SQL query
            
            Prompt rules (STRICT):
            - Prompt MUST be an affirmative IMPERATIVE statement
            - Prompt MUST start with a verb in imperative form (e.g., Specify, Define, Select, Apply, Filter, Set)
            - Prompt MUST NOT contain question words or decision phrases
              (FORBIDDEN: if, whether, decide, choose, should, do you, can you, would you)
            - Prompt MUST NOT end with a question mark
            - Prompt MUST instruct the user to provide filter criteria
            - Prompt MUST be business-friendly and reusable as LLM input
            - Prompt MUST NOT reference SQL, tables, or column names
            - Prompt MUST include a placeholder for user to provide input for filter criteria

            
            Formatting rules:
            - Use exactly one pipe character (|)
            - No numbering
            - No bullet points
            - No extra text before or after the 6 lines ";

            List<string> response = await _chatClientService.GenrateSuggestedPromt(prompt, columns);

            List<SuggestedQuestionsDTO> suggestedPrompts = response.Select(item =>
            {
                var parts = item.Split('|', 2);
                return new SuggestedQuestionsDTO
                {
                    Prompt = parts[0].Trim(),
                    Question = parts.Length > 1 ? parts[1].Trim() : string.Empty
                };
            })
            .ToList();

            return suggestedPrompts
                           .Skip(1)
                          .Take(4)
                          .ToList();
        }
    }   
}
