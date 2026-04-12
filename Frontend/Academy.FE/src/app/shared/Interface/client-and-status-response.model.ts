export interface GroupClientAndStatusResponse {
    aiStudioGroups: AiStudioGroup[];
    clients: Client[];
    detailedStatuses: DetailedStatus[];
}

export interface ClientAndStatusResponse {
    clients: Client[];
    detailedStatuses: DetailedStatus[];
}

export interface AiStudioGroup {
    id: number;
    groupName: string;
    groupNameCount?: number
}

export interface Client {
    id: number;
    client: string;
    clientCountValue?: number;
}

export interface DetailedStatus {
    id: number;
    statusName: string;
    statusNameCount?: number;
}
