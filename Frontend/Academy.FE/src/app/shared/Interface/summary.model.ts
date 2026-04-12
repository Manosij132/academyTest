export interface PivotRow {
    ticketStatus: string;
    totals: Record<string, number>;
    children: PivotChild[];
    expanded?: boolean;
}

export interface PivotChild {
    client: string;
    totals: Record<string, number>;
}

export interface SummaryFilterRequest {
    groupNames: string[];
    clients: string[];
    statuses: string[];
    startDateFrom?: string | null;
    startDateTo?: string | null;
}

export interface SummaryTicketRequest {
    groupNames?: string[];
    client?: string[];
    detailedStatuses?: string[];
    ticketStatus?: string[];
    monthClosure?: string[];
    startDateFrom?: string | null;
    startDateTo?: string | null;
    pageNumber?: number;
    pageSize?: number;
}

