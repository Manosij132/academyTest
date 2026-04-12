import { Injectable } from "@angular/core";
import { Seniority } from "../components/agk-panel-availability/model/seniority.model";
import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { environment } from '../../environments/environment';
import {map,timeout} from 'rxjs/operators';
import { Community } from "../components/agk-panel-availability/model/community.model";
import { PanelFilter } from "../components/agk-panel-availability/model/panel-filter.model";
import { PanelDashboardList } from "../components/agk-panel-availability/model/panel-dashboard.model";
import { Panel } from "../components/agk-panel-availability/model/panel.model";
import { TDC } from "../components/agk-panel-availability/model/tdc.model";
import { PanelSlotDetailModel } from "../components/agk-panel-availability/model/panel-slot-detail.model";
import { DashboardDataModel } from "../components/agk-panel-availability/model/dashboard-data.model";
import { DashboardFilterModel } from "../components/agk-panel-availability/model/dashboard-filter.model";
import { panelSlotsCalenderEvent } from "../components/agk-panel-availability/model/panelSlotsCalenderEvent.model";
import { PanelSlotDataModel } from "../components/agk-panel-availability/model/panel-slot-data.model";

@Injectable({
    providedIn: 'root'
  })

export class PanelService {
    //private baseUri = 'https://localhost:44305' + '/api/';
    private baseUri = environment.apiBaseUrl + "/" + environment.apiExtension;
    //private baseUri: string = "";
    private httpOptions : any;

    constructor(private httpClient: HttpClient){
        this.httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Methods':'GET,HEAD,OPTIONS,POST,PUT',
                'Access-Control-Allow-Headers':'Access-Control-Allow-Headers, Access-Control-Allow-Origin, Origin, Accept, X-Requested-With, Content-Type, Access-Control-Request-Method, Access-Control-Request-Headers',
                'timeout':'10800000'
            })
        }
    }

    executePost(requestUri: string,requestData : any){
        console.log(requestData);
        return this.httpClient.post(this.baseUri + requestUri,requestData,this.httpOptions).pipe(timeout(1080000));
    }

    getSeniorityData(){
        return this.httpClient.get<Seniority[]>(this.baseUri+'/InterviewPanel/GetAllSeniorityData').pipe(map(data => data));
    }

    getCommunityData(){
        return this.httpClient.get<Community[]>(this.baseUri+'/InterviewPanel/GetAllCommunityData').pipe(map(data => data));
    }

    getAllPanelData(){
        return this.httpClient.get<Panel[]>(this.baseUri+'/InterviewPanel/GetAllPanelData').pipe(map(data => data));
    }
    
    getTDCData(){
        return this.httpClient.get<TDC[]>(this.baseUri+'/InterviewPanel/GetAllTDCData').pipe(map(data => data));
    }

    getPanelData(pageSize : number,pageNumber : number,filter : PanelFilter){
        return this.executePost('/InterviewPanel/GetAllInterviewPanelsByFilterAsync?PageNumber='+pageNumber+'&PageSize=' + pageSize,filter).pipe(map(res => new PanelDashboardList(res)));
    }
    getPanelSlotDetail(panelId: number)
    {
        return this.httpClient.get<PanelSlotDetailModel[]>(this.baseUri+'/InterviewPanel/GetPanelSlotsDetail?panelId='+panelId).pipe(map(data => data));
    }
    panelSendEmail(panelEmail: any)
    {
        return this.executePost('/InterviewPanel/SendEmail',panelEmail).pipe(map(res => res));
    }

    getDashboardData(filter : DashboardFilterModel) {
        var result =  this.executePost('/InterviewPanel/GetDashboardData',filter).pipe(map(res => new DashboardDataModel(res)));
        return result;
    }

    GetInterviewPanelDetails(filter : DashboardFilterModel) {
        var result =  this.executePost('/InterviewPanel/GetInterviewPanelDetails',filter).pipe(map(res => new DashboardDataModel(res)));
        return result;
    }


    insertScheduleInterviewData(data: panelSlotsCalenderEvent) {
        return this.executePost('/InterviewPanel/SavePanelSlotCalenderEvent',data).pipe(map(res => res));
    }

    getPanelSlotDataById(slotId: number)  {
        return this.httpClient.get<PanelSlotDataModel>(this.baseUri+'/InterviewPanel/GetPanelSlotDataById?slotId='+ slotId).pipe(map(data => data));
    }

    PanelAIEvaluation(email: any) {
        let params = new HttpParams()
            .set("panelEmail", email);

        return this.httpClient.get<any>(this.baseUri + "/InterviewPanel/PanelAIEvaluation",{ params: params });
    }
}
