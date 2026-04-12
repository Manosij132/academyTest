export interface UpdateTicketDto {
  deatiledStatus?: string;
  ticketStatus?: string;
  comments?: string;
  monthClosure?: string; 
}

export interface TicketQueryParams {
  dateField: string;
  startDate?: string;
  endDate?: string;
  searchText: string;
  pageNumber: number;
  pageSize: number;
}

export interface TicketPagedResponse {
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  data: any[];
}