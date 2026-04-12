export const TrainingStatus = [
  { Key: 1, Value: "Pending", cssClass: "bg-danger" },
  { Key: 2, Value: "Completed", cssClass: "bg-success" },
  { Key: 3, Value: "Ongoing", cssClass: "bg-warning" },
  { Key: 4, Value: "Deferred", cssClass: "bg-dark" },
];

export const CompletedTrainingStatus = 2;

export const ProficiencyMaster = [
  { Key: 1, Value: "Can't Perform", Html: "" },
  { Key: 2, Value: "With Supervision", Html: "" },
  { Key: 3, Value: "With Limited Supervision", Html: "" },
  { Key: 4, Value: "Without Supervision", Html: "" },
  { Key: 5, Value: "Can Teach Others", Html: "" },
];

export const KnowledgeMaster = [
  { Key: 1, Value: "Novice", Html: "" },
  { Key: 2, Value: "Beginner", Html: "" },
  { Key: 3, Value: "Intermediate", Html: "" },
  { Key: 4, Value: "Advanced", Html: "" },
  { Key: 5, Value: "Expert", Html: "" },
];

export const Seniority = [
  { Id: 1, Text: "Tech Director" },
  { Id: 2, Text: "Tech Manager" },
  { Id: 3, Text: "Subject Matter Expert" },
  { Id: 4, Text: "Architect" },
  { Id: 5, Text: "Sr Level 3" },
  { Id: 6, Text: "Software Designer" },
  { Id: 7, Text: "Sr Level 2" },
  { Id: 8, Text: "Sr" },
  { Id: 9, Text: "Sr Level 1" },
  { Id: 10, Text: "SSr Adv" },
  { Id: 11, Text: "SSr" },
  { Id: 12, Text: "Jr Adv" },
  { Id: 13, Text: "Jr" },
  { Id: 14, Text: "NA" },
  { Id: 15, Text: "Studio Partner" },
];

export const RequestStatus = [
  { Key: 1, Value: "Pending", cssClass: "text-bg-info" },
  { Key: 2, Value: "Completed", cssClass: "text-bg-success" },
  { Key: 3, Value: "Ongoing", cssClass: "text-bg-warning" },
  { Key: 4, Value: "Error", cssClass: "text-bg-danger" },
];

export const Roles = [
  { Key: 0, Value: "User" },
  { Key: 1, Value: "System Admin" },
  { Key: 2, Value: "Tdc Admin" },
  { Key: 3, Value: "Community Admin" },
  { Key: 4, Value: "Ecosystem Admin" },
  { Key: 5, Value: "Account Admin" },
];

export enum UserRole {
  "User" = "User",
  "SystemAdmin" = "SystemAdmin",
  "TDCAdmin" = "TdcAdmin",
  "CommunityAdmin" = "CommunityAdmin",
  "EcosystemAdmin" = "EcosystemAdmin",
  "AccountAdmin" = "AccountAdmin",
}

export enum Agents {
  "Academy" = "academy",
  "Staffing" = "staffing"
}

export const TOASTER_MESSAGES = {
  SUCCESS: 'Operation completed successfully.',
  CREATE_SUCCESS: 'Created Successfully.',
  UPDATE_SUCCESS: 'Updated Successfully.',
  DELETE_SUCCESS: 'Deleted Successfully.',
  INFO: 'Please check the details.',
  ERROR: 'An error occurred. Please try again.',
  WARNING: 'Caution: This action cannot be undone.'
};
