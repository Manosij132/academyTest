export interface InterviewQuestion {
  question: string;
  answer: string;
  score: number;
}

export interface MockInterviewDetail {
  interviewId: number;
  interviewDate: string;
  skillset: string;
  avgScore: number;
  comment: string;
  questions: InterviewQuestion[];
}
export interface Question {
  questionId: number;
  questionText: string;
}

export interface InterviewData {
  interviewId: string;
  interviewDate: Date;
  skillSet: { skillName: string }[];
  avgScore: number | null;
  comment: string | null;
  scheduledDate: string;
}

export interface InterviewFilter {
  interviewStartDate: string | null;
  interviewEndDate: string | null;
}