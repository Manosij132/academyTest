export class Employee {
    id?: number = 0
    SapId?: number = 0
    SeniorityLevel?: number = 0
    MaxMentee?: number = 0
    MinMentee?: number = 0
    employeeName?: string
    globantEmailAddress?: string
    joiningDate: Date | undefined
    NotificationSendCount: number = 0
    CreatedAt: Date | undefined 
    betterMeLeaderEmail?: string
    proposedLeaderEmail?: string;
    isDeleted: boolean = false;
    seniority:string = "";
    seniorityShortName:string = "";
    designation:string= "";
    client:string = "";
    project:string = "";
    seniorityId:number=0;
    seniorityName: string = "";
    leaderAssignDate: string = "";
    inOut:boolean = false;
    inOutDate:Date| null = null;
}
