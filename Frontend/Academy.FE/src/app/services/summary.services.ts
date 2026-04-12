import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SummaryResponse, TicketDataResponse } from '@shared/Interface/summary-response.model';
import { SummaryFilterRequest, SummaryTicketRequest } from "@shared/Interface/summary.model";
import { ClientAndStatusResponse, GroupClientAndStatusResponse } from "@shared/Interface/client-and-status-response.model";
import { GroupClientFilterRequest } from '@shared/Interface/group-client-filter-request.model';
import { environment } from '@environments/environment';

@Injectable({
    providedIn: 'root'
})
export class SummaryService {
    private apiUrl = environment.staffingApiBaseurl + '/Summary';

    constructor(private http: HttpClient) { }

    getDropdownData(startDate: Date | null, endDate: Date | null): Observable<any> {
        const params = new HttpParams()
            .set('startDateTxt', this.formatDate(startDate))
            .set('endDateTxt', this.formatDate(endDate));

        return this.http.get<any>(this.apiUrl, { params });
    }

    getFilteredData(
        groupNames: string[] = [],
        clients: string[] = [],
        statuses: string[] = [],
        startDateFrom?: Date | null,
        startDateTo?: Date | null
    ) {
        const payload: SummaryFilterRequest = {
            groupNames,
            clients,
            statuses,
            startDateFrom: startDateFrom ? this.formatDate(startDateFrom) : null,
            startDateTo: startDateTo ? this.formatDate(startDateTo) : null
        };

        return this.http.post<SummaryResponse>(
            `${this.apiUrl}/GetFilteredData`,
            payload
        );
    }

    getTicketData(groupNames: string[] = [], client: string[] = [], detailedStatuses: string[] = [],
        ticketStatus: string[] = [],
        monthClosure: string[] = [],
        startDateFrom?: Date | null,
        startDateTo?: Date | null,
        pageNumber: number = 1,
        pageSize: number = 25) {

        const payload: SummaryTicketRequest = {
            groupNames,
            client,
            detailedStatuses,
            ticketStatus,
            monthClosure,
            startDateFrom: startDateFrom ? this.formatDate(startDateFrom) : null,
            startDateTo: startDateTo ? this.formatDate(startDateTo) : null,
            pageNumber,
            pageSize
        };

        return this.http.post<TicketDataResponse>(`${this.apiUrl}/GetFilteredTicketData`, payload);
    }

    getClientAndDetailedStatusByAIGroup(
        groupNames: string[], startDateTxt: Date | null, endDateTxt: Date | null
    ): Observable<ClientAndStatusResponse> {

        const params = new HttpParams()
            .set('startDateTxt', this.formatDate(startDateTxt))
            .set('endDateTxt', this.formatDate(endDateTxt));

        return this.http.post<ClientAndStatusResponse>(
            `${this.apiUrl}/GetClientAndDetailedStatusByAIGroup`,
            groupNames,
            { params }
        );
    }

    getDetailedStatusByAIGroupAndClient(
        groupNames: string[],
        clients: string[],
        startDateFrom: Date | null,
        startDateTo: Date | null
    ): Observable<GroupClientAndStatusResponse> {

        const payload: GroupClientFilterRequest = {
            groupNames,
            clients,
            startDateFrom,
            startDateTo
        };

        return this.http.post<GroupClientAndStatusResponse>(
            `${this.apiUrl}/GetDetailedStatusByAIGroupAndClient`,
            payload
        );
    }

    private formatDate(date: Date | null): string {
        if (!date) return '';
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

}