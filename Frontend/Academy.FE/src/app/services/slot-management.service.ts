import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { SlotManagementModel } from '../components/agk-panel-availability/model/slot-management.model';
import { SlotManagementFilter } from '../components/agk-panel-availability/model/slot-mgmt-filter.model';
import { Observable } from 'rxjs';
import { CommunitySelectionRatio } from '../components/agk-panel-availability/model/community-selection-ratio.model';
import { PredictedSelectionRatioModel } from '../components/agk-panel-availability/model/predicated-selection-ratio.model';

@Injectable({
  providedIn: 'root'
})
export class SlotManagementService {
  private webAPIDapperBaseURL = environment.apiBaseUrl + "/" + environment.apiExtension;
  //private webAPIDapperBaseURL = 'https://localhost:44305/api/SlotRequirement/';

  constructor(private http: HttpClient) { }

  public getAllSlotManagementData(slotmanagementFilter : SlotManagementFilter) {
    let params = new HttpParams().set("TDC",slotmanagementFilter.tDCs).set("communityID",slotmanagementFilter.communityID).set("startDate",slotmanagementFilter.startDate).set("endDate",slotmanagementFilter.endDate);
    var result = this.http.get<SlotManagementModel[]>(this.webAPIDapperBaseURL + "/SlotRequirement/GetAllSlotManagementData",{params: params});
    console.log(result);
    return result;
  }

  public UpdateSlotManagement(slotManagementModel: Array<SlotManagementModel>): Observable<any> {
    const headers = { 'content-type': 'application/json' }
    const body = JSON.stringify(slotManagementModel);
    return this.http.post(this.webAPIDapperBaseURL + "/SlotRequirement/UpdatePanelSlotRequirement", body, { 'headers': headers })
  }

  public CreateSlotManagement(slotManagementModel: Array<SlotManagementModel>): Observable<any> {
    const headers = { 'content-type': 'application/json' }
    const body = JSON.stringify(slotManagementModel);
    return this.http.post(this.webAPIDapperBaseURL + "/SlotRequirement/CreatePanelSlotRequired", body, { 'headers': headers })
  }

  public getCommunitySelectionRatio(slotmanagementFilter : SlotManagementFilter) {
    let params = new HttpParams().set("TDC",slotmanagementFilter.tDCs).set("communityID",slotmanagementFilter.communityID).set("startDate",slotmanagementFilter.startDate).set("endDate",slotmanagementFilter.endDate);
    return this.http.get<CommunitySelectionRatio>(this.webAPIDapperBaseURL + "/SlotRequirement/GetCommunitySelectionRatio",{params: params});
  }

  public getPredictedSelectionRatio(slotmanagementFilter : SlotManagementFilter) {
    let params = new HttpParams().set("TDC",slotmanagementFilter.tDCs).set("communityID",slotmanagementFilter.communityID).set("startDate",slotmanagementFilter.startDate).set("endDate",slotmanagementFilter.endDate);
    return this.http.get<PredictedSelectionRatioModel>(this.webAPIDapperBaseURL + "/SlotRequirement/GetPredicatedRatio",{params: params});
  }

  public UpdateCommunitySelectionRatio(communitySelectionRatio: CommunitySelectionRatio): Observable<any>
  {
    const headers = { 'content-type': 'application/json' }
    const body = JSON.stringify(communitySelectionRatio);
    return this.http.post(this.webAPIDapperBaseURL + "/SlotRequirement/UpdateCommunitySelectionRatio", body, { 'headers': headers })
  }

}
