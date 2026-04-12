import { Community } from "./community.model";
import { Panel } from "./panel.model";
import { Seniority } from "./seniority.model";
import { TDC } from "./tdc.model";

export class DashboardSlotFilter {   
    tdcs: TDC[] = [];
    communities: Community[] = [];
    senorities: Seniority[] = [];
    panelTypes: Panel[] = [];
    startDate : Date = new Date();
    endDate : Date = new Date();
    searchTerm : string ="";
}