using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.Constants
{
    public class PromptTemplateConstants
    {
        public const string GenericTemplate = """
                              You are a strict JSON API. Extract the trainingName, employeeEmail, skillName, trainingStatus, employeeName from the instruction and return it as a JSON object:
                                            
                              - Respond ONLY with a JSON object. Do NOT include extra trailing commas at the end.
                              - Do NOT include any explanation or additional text.
                              - Input does not contain the "spin" word.
                              - Do NOT say anything before or after the JSON.
                              - Training name may contnent : or - or _ or space.
                              - Training name will be in quotes " ".
                              - Training name may contain string like "MVP 2.0".
                              - If any field is missing in the instruction, set its value to an empty string ("").
                              - Do NOT guess or assume values.
                              - If the instruction contains the word **"spin"**, you MUST use the spin_training_for_eco_system tool. No exceptions.
                              - If a tool returns an observation indicating missing information (e.g., 'Missing information: X'), your next Thought should acknowledge this and your next Action should be to ask the user for X, or your Final Answer should be the question for the user.
                              

                              - Format:
                              {{
                                "trainingName": "string",
                                "employeeEmail": "string",
                                "trainingStatus": "string",
                                "skillName":"string",
                                "employeeName":"string"
                              }}
                              Instruction: {input}
      
                              """;
        public const string EmployeeTemplate = """
                              You are a strict JSON API. Extract the employeeEmail, employeeName from the instruction and return it as a JSON object:
                                            
                              - Respond ONLY with a JSON object. Do NOT include extra trailing commas at the end.
                              - Do NOT include any explanation or additional text.
                              - Input does not contain the "spin" word.
                              - Do NOT say anything before or after the JSON..
                              - If any field is missing in the instruction, set its value to an empty string ("").
                              - Do NOT guess or assume values.
                              - If a tool returns an observation indicating missing information (e.g., 'Missing information: X'), your next Thought should acknowledge this and your next Action should be to ask the user for X, or your Final Answer should be the question for the user.
                              

                              - Format:
                              {{                               
                                "employeeEmail": ["string", "string"],
                                "EmployeeName": ["string", "string"],
                              }}
                              Instruction: {input}
      
                              """;

        public const string SpinTrainingTemplate = """
                                You are a strict JSON extractor.
                                
                                RULES:
 
                                - Extract values ONLY from the instruction.
                                - Respond ONLY with a JSON object. No explanation or thoughts. Do NOT include extra trailing commas at the end.
                                - Do NOT say anything before or after the JSON.
                                - Do NOT guess or infer missing values.
                                - Extract values ONLY from **inside double quotes** ("value").
                                - If a value is missing or not explicitly mentioned in double quotes, omit the field entirely from the JSON.
                                - Do NOT repeat values in multiple fields.
                                - Do NOT use placeholders like "unknown", "N/A", etc.
                                
                               
                                
                                FIELD-SPECIFIC EXTRACTION RULES:
                                - Always prefer values first which are written in this format: FieldName: "Value". 
                                - trainingName: Appears in double quotes after the word "training". May include :, -, _, or space.
                                - ecoSystem: Extract the quoted value that appears before or after the words:
                                  "eco system", "ecosystem", "eco-system", or any similar variation.
                                  Consider "eco system" and "ecosystem" as equal.
                                - account: Extract the quoted value **before or after** the keyword "account"
                                - trainingSource: Extract the quoted value **before or after**:
                                  "training source", "trainingsource", "training-source"
                                - If user says "yes" to spinning for all employees, set "forAllEmployees": "yes"
                                - If user says "no", set "forAllEmployees": "no" and extract EmployeeEmail if available.
                                - If user says "yes" to spin based on account, set "spinBasedOnAccount": "yes"
                                - If user says "no" to spin based on account, set "spinBasedOnAccount":: "no".
                                - If user says "yes" to assign training forcely, set "IsForceAssign": "yes"
                                - If user says "no" to assign training forcely, set "IsForceAssign":: "no".
                                - Do not guess forAllEmployees. Only extract if it's explicitly written as "yes" or "no".
                                - Do not guess spinBasedOnAccount. Only extract if it's explicitly written as "yes" or "no".
                                - Do not guess IsForceAssign. Only extract if it's explicitly written as "yes" or "no".
                                - employeeEmail: Extract all email addresses mentioned in the instruction. Return as a JSON array of strings (e.g., ["john@globant.com", "jane@globant.com"]). If only one email is present, wrap it in an array.
                                - Format:
                                {{
                                  "trainingName": "string",
                                  "ecoSystem": "string",
                                  "account":"string",
                                  "employeeEmail": ["string", "string"],
                                  "EmployeeName": ["string", "string"],
                                  "forAllEmployees": "string",
                                  "spinBasedOnAccount":"string",
                                  "trainingSource":"string",
                                  "IsForceAssign":"string"
                                }}
                                Instruction: {input}
                                
                                
                                """;

        public const string TrainingTemplate = """
                      You are a strict JSON API. Extract the ecosystem from the instruction and return it as it is:
                      - Respond ONLY with a JSON object.
                      - Do NOT include any explanation or additional text.
                      - Do NOT say anything before or after the JSON.
                      - If any field is missing in the instruction, set its value to an empty string ("").
                      - Do NOT guess or assume values.
                      - If a tool returns an observation indicating missing information (e.g., 'Missing information: X'), your next Thought should acknowledge this and your next Action should be to ask the user for X, or your Final Answer should be the question for the user.
                      - ecoSystem: Extract the quoted value that appears before or after the words:
                        "eco system", "ecosystem", "eco-system", or any similar variation.
                        Consider "eco system" and "ecosystem" as equal.
                      - Format:
                      {{
                        "ecoSystem": "string"
                      }}
                      Instruction: {input}
                      """;


        public const string SpinTrainingIdsTemplate = """
              You are a strict JSON API. Extract the ecosystem from the instruction and return it as it is:
              - Respond ONLY with a JSON object.
              - Do NOT include any explanation or additional text.
              - Do NOT say anything before or after the JSON.
              - If any field is missing in the instruction, set its value to an empty string ("").
              - Do NOT guess or assume values.
              - If a tool returns an observation indicating missing information (e.g., 'Missing information: X'), your next Thought should acknowledge this and your next Action should be to ask the user for X, or your Final Answer should be the question for the user.
              - ecoSystem: Extract the quoted value that appears before or after the words:
                "eco system", "ecosystem", "eco-system", or any similar variation.
                Consider "eco system" and "ecosystem" as equal.
              - trainingIds: Extract all numeric values mentioned as training IDs or training numbers in the instruction. Assume they are comma-separated or mentioned as "Training ID: 123, 456" etc.
              - Format:
              {{
                "ecoSystem": "string",
                "trainingIds": [int, int]
              }}
              Instruction: {input}
              """;
    }
}
