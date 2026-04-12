
export class panelSlotsCalenderEvent {
    id: number;
    slotDate: Date; 
    recruiter: string;
    candidateName: string;
    candidateEmail: string;
    fileEncoded: string;
    loggedInUserEmailID: string;
    resumeFileName: string;
    eventTitle: string;
    targetIanaTimeZoneId: string;
    

    constructor(id: number, slotDate: Date, recruiter: string, candidateName: string,candidateEmail: string, fileEncoded: string, loggedInUserEmailID: string, resumeFileName: string, eventTitle: string,targetIanaTimeZoneId:string) {
        this.id = id;
        this.slotDate = slotDate;
        this.recruiter = recruiter;
        this.candidateName = candidateName;
        this.candidateEmail = candidateEmail;
        this.fileEncoded = fileEncoded;
        this.loggedInUserEmailID = loggedInUserEmailID;
        this.resumeFileName = resumeFileName;
        this.eventTitle = eventTitle;
        this.targetIanaTimeZoneId = targetIanaTimeZoneId;
    }
}