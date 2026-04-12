export const MY_UTC_FORMATS = {
  parse: {
    dateInput: 'YYYY-MM-DD', // Uppercase Y, M, D for Moment.js
  },
  display: {
    dateInput: 'YYYY-MM-DD',   // How the date displays in the input
    monthYearLabel: 'MMM YYYY', // Format for the calendar header
    dateA11yLabel: 'LL',        // Accessibility label (full date)
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

export enum EnumReportType {
  DetailReport = 1,
  Summary = 2,
  Compliance = 3,
}