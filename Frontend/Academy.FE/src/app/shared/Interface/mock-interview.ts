
export interface Seniority {

    seniorityId: number;
    seniorityLevel: string;
    seniorityName: string;

}

export interface Skill {

    skillId: number;
    skillName: string;

}

export interface ScheduleInterviewRequest {
    adminId: number,
    employeeId: number,
    seniorityId: number,
    skillId: string[],
    scheduledDate: string
}

export interface InterviewData {
    interviewId:any
    scheduledDate: Date
    skillset: string | string[]
    avgScore: number
    comment: string
    interviewDate: Date
}