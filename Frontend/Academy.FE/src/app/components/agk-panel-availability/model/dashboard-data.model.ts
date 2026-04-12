import { InterviewScheduleData } from "../interview-schedule/heatmap-table/heatmap-table.model";
import { ChartDataModel } from "./chartdata.model";
import { DashboardTileModel } from "./dashboard-tile.model";
import { PanelGrid } from "./panel-grid.model";

export class DashboardDataModel
{
    dashboardTiles: DashboardTileModel | undefined;
    communityChartDataModel: ChartDataModel[] = [];
    panelTypeChartDataModel: ChartDataModel[] = [];
    panelists: InterviewScheduleData[] = [];
    startDate: Date | undefined;
    endDate: Date | undefined;


    constructor(obj?: any){
        Object.assign(this,obj);
    }
}
