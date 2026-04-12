export class PanelSlotDataModel {
    public slotDate: Date;               
    public recruiter: string;             
    public candidateName: string;         
    public candidateEmail: string;        
    public fileEncoded: string;           
    public loggedInUserEmailID: string;   
    public resumeFileName: string;        
    public eventTitle: string;            
    constructor(
        slotDate: Date,
        recruiter: string,
        candidateName: string,
        candidateEmail: string,
        fileEncoded: string,
        loggedInUserEmailID: string,
        resumeFileName: string,
        eventTitle: string
    ) {
        this.slotDate = slotDate;
        this.recruiter = recruiter;
        this.candidateName = candidateName;
        this.candidateEmail = candidateEmail;
        this.fileEncoded = fileEncoded;
        this.loggedInUserEmailID = loggedInUserEmailID;
        this.resumeFileName = resumeFileName;
        this.eventTitle = eventTitle;
    }
}