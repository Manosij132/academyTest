using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Models
{
    public enum StructuredAgent
    {
        None,
        Employee,
        Attendence,
        Staffing
    }

    public static class AgentPrompts
    {
        public static void AddPromptsExample(StringBuilder builder, StructuredAgent agentName)
        {
            switch (agentName)
            {
                case StructuredAgent.Employee:
                    builder.Append("Here are some sample values for the columns:");
                    builder.Append(@"\t[EmployeeId] [1213555],\r\n\t[EmployeeName] [Amar Shaikh],\r\n\t[Email] [amar.shaikh@globant.com],\r\n\t[Vertical] [],\r\n\t[Studio] [Engineering],\r\n\t[Community] [DOTNET],\r\n\t[CareerLeaderEmail] [akash.shinde@globant.com],\r\n\t[AccountLeaderEmail] [akash.shinde@globant.com],\r\n\t[Image] [assets/images/general.jpg],\r\n\t[IsActive] [True],\r\n\t[SystemRole] [0],\r\n\t[Position] [.NET developer],\r\n\t[CommunityLeader] [gaurav.zambare@globant.com],\r\n\t[CareerLeaderAssignedDate] [01-01-2025 18:00:00],\r\n\t[BaseLocation] [Hinjewadi],\r\n\t[Client] [Globant],\r\n\t[Gender] [Male],\r\n\t[JoiningDate] [01-Jan-2025],\r\n\t[Project] [Fast360],\r\n\t[Seniority] [Sr],\r\n\t[TotalExperience] [3.2],\r\n\t[CareerLeader] [Tanmay Joshi],\r\n\t[AccountLeader] [Sachin Pawar]");
                    builder.AppendLine("If there is a question based on TotalExperience column, use the CONVERT(float, TotalExperience) column to convert this column to float. E.g. SELECT COUNT(EmployeeId) 'Number of Employees' FROM dbo.EmployeeDetails WHERE CONVERT(float, TotalExperience) > 3;");
                    builder.AppendLine("Ignore SystemRole and Vertical columns.");
                    break;
                case StructuredAgent.Attendence:
                    builder.Append("Here are some sample values for the columns:");
                    builder.Append(@"\t[Id] [7CC75C59-AF33-4B5B-396A-08DD316EA16B]	,\r\n\t[Name] [Pravesh Chouksey],\r\n\t[Email] [Pravesh.Chouksey@globant.com],\r\n\t[Date] [2025-01-09 00:00:00.0000000],\r\n\t[Location] [Indore],\r\n\t[Studio] [Engineering]	,\r\n\t[Account] [AIB],\r\n\t[Project] [Mobile 4],\r\n\t[Week] [1],\r\n\t[Source] [Wifi],\r\n\t[ProcessDate] [2025-01-10 00:00:00.0000000],\r\n\t[BaseLocation] [NULL]");
                    builder.AppendLine("Ignore Id and BaseLocation columns");
                    break;
                case StructuredAgent.Staffing:
                    builder.Append("If the user input has misspelled or shortened table/column names, correct them considering schema and still produce valid SQL. Also if required perform the joins and get the actual values instead of Ids(Primary/foreign Keys)");
                    builder.Append("Here are some sample values for the columns and these are just sample column values, do not use them in where clause to filter the records:");
                    builder.Append(@"\t[RequestID], [Client], [Project], [Name], [PositionID], [Seniority], [ContractorAllowed], [WorkOfficeID], [PositionFramework], [Skills], [ClientInterviewRequired], [Plans], [GloberToBeAssigned], [Stage], [StartDate], [Load], [Replacement], [RateAmount], [RatePeriod], [Handler], [AssociateHandler], [PositionStudio], [TypeOfPosition], [EnglishRequired], [HandlerTeam], [AssociateHandlerTeam], [BusinessUnit], [Region], [Submitter], [SubmitDate], [Aging], [EstimatedStaffingDate], [SecondaryLocation], [PositionNotes], [Skills], [Plans]");
                    builder.Append(@"\r\n\t[RequestID] [596962], \r\n\t[Client] [Disney Entertainment], \r\n\t[Project] [AP LI AdVisor Sales], \r\n\t[Name] [Disney Entertainment - Advisor TAE], \r\n\t[PositionID] [], \r\n\t[Seniority] [Sr Level 1], \r\n\t[ContractorAllowed] [No], \r\n\t[WorkOfficeID] [MX], \r\n\t[PositionFramework] [N/A], \r\n\t[Skills] [Api Automated Testing 4★; Test Case Design 4★; Testing Coverage Analysis 3★], \r\n\t[ClientInterviewRequired] [Yes], \r\n\t[Plans] [Marco A G d L H - Candidate - High; Ulises F S A - Candidate - High], \r\n\t[GloberToBeAssigned] [SAMUEL MARTINEZ ARREOLA - Full Time Glober], \r\n\t[Stage] [1], \r\n\t[StartDate] [28/04/2025], \r\n\t[Load] [100 %], \r\n\t[Replacement] [No], \r\n\t[RateAmount] [59.32], \r\n\t[RatePeriod] [Hour], \r\n\t[Handler] [Ana Maria Ramirez Carrillo], \r\n\t[AssociateHandler] [Marcelo Carrazco Duran], \r\n\t[PositionStudio] [Engineering], \r\n\t[TypeOfPosition] [Billed], \r\n\t[EnglishRequired] [No], \r\n\t[HandlerTeam] [Colombia/Peru], \r\n\t[AssociateHandlerTeam] [Recruiting - NACA], \r\n\t[BusinessUnit] [Disney Media], \r\n\t[Region] [MESH], \r\n\t[Submitter] [Leticia Casarreal], \r\n\t[SubmitDate] [17/12/2024], \r\n\t[Aging] [101], \r\n\t[EstimatedStaffingDate] [N/A], \r\n\t[SecondaryLocation] [CO,EC,AR,PE], \r\n\t[PositionNotes] [TAE + Python (por favor no sugerir si no tiene experiencia previa comprobada en Python) | Gina Esther Segovia Anculle - 31-Mar-2025 15:23; TAE + Python (por favor no sugerir si no tiene Python) | Gina Esther Segovia Anculle - 17-Mar-2025 15:24], \r\n\t[Skill] [Python], \r\n\t[Plans] [TAE + Python]");
                    builder.Append("Generate a SQL SELECT query on table [StaffRequests] with filters applied. \r\nUse the LIKE operator with wildcards (%) for most string columns such as [Client], [Project], [Name], [PositionID], [Seniority], [WorkOfficeID], [PositionFramework], [Skills], [Plans], [Handler], [AssociateHandler], [PositionStudio], [TypeOfPosition], [HandlerTeam], [AssociateHandlerTeam], [BusinessUnit], [Region], [Submitter], [SecondaryLocation], [PositionNotes], [Skills], and [Plans].\r\n\r\n" +
                        "For numeric  columns (e.g., [RequestID], [Stage], [StartDate], [RateAmount], [Aging], ) use exact match (=) or range conditions instead of LIKE along with other columns.\r\n\r\nExample format:\r\nSELECT *\r\nFROM [StaffRequests]\r\nWHERE [Client] LIKE '%Disney%'\r\n  AND [Project] LIKE '%Advisor%'\r\n  AND [Seniority] LIKE '%Sr Level%'" +
                        "FOR date columns (e.g. [StartDate], [SubmitDate], [EstimatedStaffingDate]) use range conditions for example BETWEEN, <=, >=, <, > \r\n.");
                    builder.Append("Consider seniority column when position mentioned. Always use LIKE operator with wildcards (%) for [Client] Column.");
                    break;
                default:
                    break;
            }

        }


        public static void PromptRefinement(StringBuilder stringBuilder)
        {
            var mapping = GetTextMapping();
            stringBuilder.AppendLine("Here are some synonyms. Replace the word on the left with the word on the right if you encounter any of them.");
            foreach (var pair in mapping)
            {
                stringBuilder.Append($" {pair.Key}={pair.Value} ");
            }
        }

        private static Dictionary<string, string> GetTextMapping()
        {
            return new Dictionary<string, string> {
                { "GX Leader", "Experience Leader or Account Leader"},
                { "Manager", "Experience Leader or Account Leader"},
                { "Career Mentor", "Career Leader"},
                { ".NET", "DOTNET"},
                { "location", "base location OR WorkOfficeID OR SecondaryLocation"},
                { "role", "seniority"},
                { "glober", "employee"},
                { "month", "StartDate between"},
                { "Open", "Stage == 2"},
                { "closed", "Stage == 1"},
                { "office", "WorkOfficeID OR SecondaryLocation"},
                { "position", "PositionID"},
                { "framework", "PositionFramework"},
                { "Submitted by", "Submitter" },
                { "submitted on", "SubmitDate" },
                { "english is mandatory", "EnglishRequired" },
                { "Rate", "RateAmout/RatePeriod" },
                { "charge", "RateAmout/RatePeriod" },
                { "Interview Required","ClientInterviewRequired" },
                { "handler","Handler OR AssociateHandler OR HandlerTeam OR AssociateHandlerTeam like % %" },
                { "notes", "PositionNotes" },
                { "comments", "PositionNotes" },
                { "end date", "EstimatedStaffingDate" },
                { "staffing date", "EstimatedStaffingDate" },
                { "assigned", "GloberToBeAssigned" },
                { "experience level", "Seniority" },
                { "as no plan or as no plans", "Plans IS NULL OR Plans = '' OR Plans = 'No Plan'" },
                { "not having plan", "Plans IS NULL OR Plans = '' OR Plans = 'No Plan'" },
                { "as plan", "Plans IS NOT NULL AND Plans <> '' AND Plans <> 'No Plan'" },
                { "having plan ", "Plans IS NOT NULL AND Plans <> '' AND Plans <> 'No Plan'" },
                { "PositionID", "Seniority" }
            };
        }
    }
}
