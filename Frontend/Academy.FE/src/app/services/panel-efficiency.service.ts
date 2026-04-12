import { Injectable } from "@angular/core";
import { Seniority } from "../components/agk-panel-availability/model/seniority.model";
import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { environment } from '../../environments/environment';
import {map,timeout} from 'rxjs/operators';
import { firstValueFrom } from "rxjs";

@Injectable({
    providedIn: 'root'
  })

export class PanelEfficiencyService {
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
    
    public async GetPanelEfficiencyDetails(data: any): Promise<any> {

        let params = new HttpParams()
            .set("startDate", data.startDate)
            .set("endDate", data.endDate);

        return await firstValueFrom(
            this.httpClient.get<any>(
            this.baseUri + "/PanelEfficiency/GetPanelEfficiency",
            { params: params }
            )
        );
    }
   
}
