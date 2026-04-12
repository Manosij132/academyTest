import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UpdateTicketDto, TicketQueryParams, TicketPagedResponse } from '@shared/Interface/ticket.model';
import { environment } from '@environments/environment';

@Injectable({ providedIn: 'root' })
export class TicketsService {
    apiBase = environment.staffingApiBaseurl + '/StaffRequests';

    constructor(private http: HttpClient) { }

    getTickets(params: TicketQueryParams) {
        let httpParams = new HttpParams()
            .set('dateField', params.dateField)
            .set('searchText', params.searchText)
            .set('pageNumber', params.pageNumber.toString())
            .set('pageSize', params.pageSize.toString());

        if (params.startDate) {
            httpParams = httpParams.set('startDate', params.startDate);
        }

        if (params.endDate) {
            httpParams = httpParams.set('endDate', params.endDate);
        }

        return this.http.get<TicketPagedResponse>(this.apiBase, { params: httpParams });
    }

    getTicketsById(id: number): Observable<any> {
        return this.http.get<any>(`${this.apiBase}/${id}`);
    }

    updateEditableTicketFields(id: number, dto: UpdateTicketDto): Observable<any> {
        return this.http.put(`${this.apiBase}/${id}`, dto);
    }
}