using System.Data;

using Microsoft.AspNetCore.Http;

namespace Academy.Core.Abstraction.Infrastructure
{
    public interface IGoogleApiManager
    {
        Task<IDictionary<string, string>> GetFileByFileId(string fileId);
        Task<Dictionary<string, IList<IList<object>>>> ReadRawDataFromWorksheetBySheetName(string worksheetId, string sheetrange);
        Task InsertFormulaByRange(string workbookId, string sheetName, IList<IList<object>> values, string range);
        Task<IList<IList<object>>> Insert(IList<IList<object>> values, string worksheetId, string range);
        Task UpdateRow(IList<IList<object>> values, string worksheetId, string range);
        Task AppendRow(IList<IList<object>> values, string worksheetId, string range);
        Task<IList<IList<object>>> Insert(string value, string worksheetId, string range);
        Task<Dictionary<int?, string>> GetSheetsInWorksheet(string worksheetId);
        Task<bool> InsertRowAt(string worksheetId, int sheetId, int rowIndex);
        Task<bool> InsertRowAt(string worksheetId, string sheetName, int rowIndex);
        Task InsertColumnAt(string worksheetId, int sheetId, int columnIndex);
        Task<int?> AddNewEmptySheetAsync(string worksheetId, string sheetName);
        Task<List<Tuple<int?, string, int>>> GetLastRowIndex(string worksheetId);
        Task RemoveSheetFromWorksheet(string worksheetId, string steetNameThatNeedsTobeRemoved);
        Task<int> CopySingleSheetFromOneWorksheetToAnother(string sourceWorksheetId, string targetWorksheetId, string sourceSheetName);
        Task CopyAllSheetFromOneWorksheetToAnotherExcept(string sourceWorksheetId, string targetWorksheetId, List<string> sheetNameToExclude);
        Task<KeyValuePair<string, string>> CreateNewWorksheet(string title);
        Task<bool> DeleteColumn(string worksheetId, int sheetId, int columnIndex);
        Task<bool> DeleteRow(string worksheetId, int sheetId, int rowIndex);
        Task<bool> DeleteColumn(string worksheetId, string sheetName, int columnIndex);
        Task<bool> DeleteRow(string worksheetId, string sheetName, int rowIndex);
        Task CreateDropdownCell(string worksheetId, int? sheetID, int startRowIndex,
            int StartColumnIndex, int endColumnIndex,
            params string[] values);
        Task<bool> RenameSheet(string worksheetId, int sheetIdWhoseNameNeedsToBeChanged, string newSheetName);
        Task<bool> MoveSheetPosition(string worksheetId, int sheetId, int newPosition);
        Task<bool> SetCellBackgroundAndForeGroundColor(string worksheetId, int sheetId, int startRowIndex, int endRowIndex, int startColumnIndex, int endColumnIndex,
            int backgroundColorBlue, int backgroundColorGreen, int backgroundColorRed, int fontColorBlue, int fontColorGreem, int fontColorRed,
            bool textBold = false, bool horizontalAlignmentCenter = false);
        Task<string> MoveFileToAnotherFolder(string resourceId, string folderId);
        Task<string> RenameFile(string resourceId, string newName);
        Task GrantPermissionTo(string resourceId, params string[] candidateEmails);
        //Task<SearchDirectoryPeopleResponse> GetPeopleDetail(string requestParameter, CancellationToken cancellationToken = default);
        Task<string> CreateFileOnDrive(string name, string type);
        Task<string> UploadFileOnDrive(string name, string type, Stream fileContent);
        Task<string> ReadFileContent(string fileId);
        Task<Dictionary<string, string>> ListAll(string folderId);
        Task ClearData(string worksheetId, int sheetId);
        Task MergeRows(string worksheetId, string sheetName, int column, int fromRow, int toRow);
        Task MergeColumns(string worksheetId, string sheetName, int row, int fromColumn, int toColumn);

        Task<string> WriteSheetDirectly(string spreadsheetId, DataTable table, string sheetName);
        Task<Dictionary<string, string>> ListAllChildFolders(string folderId);
        Task<string> CreateFolder(string folderName, string parentFolderId = null);
        Task<(DateTime modifiedOn, string webContentLink)> UploadFile(IFormFile file, string fileName, string parentFolderId = null);
        Task DeleteFile(string WebContentLink);
    }
}
