export interface AssignedThroughReportRequest {
    pageIndex: number,
    pageSize: number,
    country: string[],
    community: string[],
    aiStudio: string[],
    account: string[],
    dojoStartDate: string,
    dojoEndDate: string,
    isPrimaryRecord: boolean,
    searchText: string
}