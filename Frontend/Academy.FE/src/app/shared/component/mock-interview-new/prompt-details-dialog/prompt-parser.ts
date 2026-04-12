export interface ParsedPrompt {
  role: string;
  question: string;
  scoreGuideline: string;
  candidateAnswer: string;
  evaluationInstructions: string[];
  outputFormat: string;
}

export function parsePrompt(prompt: string): ParsedPrompt {
  const escapeRegex = (text: string) =>
    text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

  const getSection = (label: string) => {
    const escapedLabel = escapeRegex(label);
    const regex = new RegExp(
      `${escapedLabel}:([\\s\\S]*?)(?=\\n[A-Z ][A-Z ()]+:|$)`,
      "i"
    );
    return prompt.match(regex)?.[1]?.trim() || "";
  };

  return {
    role: prompt.split("\n")[1]?.trim() || "",
    question: getSection("Question"),
    scoreGuideline: getSection("Score Guideline"),
    candidateAnswer: getSection("Candidate's Answer"),
    evaluationInstructions: getSection("EVALUATION INSTRUCTIONS")
      .split("\n")
      .map((v) => v.trim())
      .filter(Boolean),
    outputFormat: getSection("OUTPUT FORMAT (JSON only)"),
  };

  
}

export function getParsedPrompt(row: any) {
  if (!row.analysis?.prompt) return null;
  return parsePrompt(row.analysis.prompt);
}
