import { PanelGrid } from "./panel-grid.model";

export class PanelDashboardList{
    pageNumber:number = 0;
    pageSize : number = 0;
    totalPages:number = 0;
    totalRecords:number = 0;
    data : PanelGrid[] = [];
    error : string = "";
    succeeded : boolean = false;
    message : any;
    totalFilteredRecords:number = 0;

    constructor(obj?: any){
        Object.assign(this,obj);
    }
}