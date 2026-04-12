export class PanelFilter{
    tDCs : string[] = [];
    communities : number[] = [];
    seniorities : number[] = [];
    panelTypes : string[] = [];
    startDate : string = "";
    endDate : string = "";
    searchTerm : string| null = "";
    availableSlots: boolean = false;
    isDeficit: boolean = false;
}