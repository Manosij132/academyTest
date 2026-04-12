export interface GroupClientFilterRequest {
    groupNames: string[];
    clients: string[];
    startDateFrom: Date | null;
    startDateTo: Date | null;
}
