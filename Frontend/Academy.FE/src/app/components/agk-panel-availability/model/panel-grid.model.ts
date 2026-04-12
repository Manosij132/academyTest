export class PanelGrid{
    id : number = 0;
    emailId : string = "";
    globantLeaderEmailId: string = '';
    panelName : string = "";
    panelType : string = "";
    seniorityId : number = 0;
    seniorityName : string = "";
    communityId : number = 0;
    communityName : string = "";
    requiredSlots: number = 0;
    slotCount : number = 0;
    nonUtilizedSlot : number = 0;
    deficit : number = 0;
    quater : string = "";
    checked: boolean = false;
    communityGKFocalEmailId: string = ""

    constructor(obj?: any){
        Object.assign(this,obj);
    }
}