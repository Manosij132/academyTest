export class SlotManagementModel {
    id: number | undefined;
    tdc: string = "";
    communityId: number | undefined;
    seniorityId: number | undefined;
    seniority: string = "";
    startDate : Date | undefined;
    endDate : Date | undefined;
    positionToBeFilled: number | undefined;
    dropRatio: number | undefined;
    offersToBeRolledOut: number | undefined;
    l1SlotsRequired: number | undefined;
    gkSlotsRequired: number | undefined;
    l1SlotsActual: number | undefined;
    gkSlotsActual: number | undefined;
    shortFallL1: number | undefined;
    shortFallGK: number | undefined;
    riskIndicator: string = "";;
    isEditing: boolean | undefined;
    l1SelectionRatio: number | undefined;
    gkSelectionRatio: number | undefined;
    l1Panels: number | undefined;
    gkPanels: number | undefined;
 
}
