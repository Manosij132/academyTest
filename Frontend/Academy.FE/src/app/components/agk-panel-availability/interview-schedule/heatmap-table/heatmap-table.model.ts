export interface PanelistAPIResponse {
  interviewScheduleData: InterviewScheduleData[];
}

export interface InterviewScheduleData {
  panelId: number;
  panel: string;
  primaryPanel: string;
  upToSeniority: string;
  communityName: string;
  emailId: string;
  slots: PanelSlot[];
}

export interface PanelSlot {
  id : number;
  isUtilized: boolean;
  slotDate: string;
}

export type SlotType = 'Utilised' | 'Unutilised' | 'Unavailable';

export interface PanelGridSlot {
  time: string;
  timeDisplay: string;
  type: SlotType;
  id: number;
}

export interface PanelistGridModel extends Omit<InterviewScheduleData, 'slots'> {
  slots: Record<string, PanelGridSlot>;
}

export interface HeatmapTableColumn {
  key: string;
  day: string;
  date: number;
}

export interface HeatmapTableData extends Omit<InterviewScheduleData, 'slots'> {}
