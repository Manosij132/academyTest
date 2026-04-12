using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Services;
using Academy.Shared.DTO;
using Academy.Shared.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Data;
using File = Google.Apis.Drive.v3.Data.File;
namespace Academy.Infrastructure.GoogleClient
{
    public class GoogleApiManager : IGoogleApiManager
    {
        private readonly SheetsService sheetsService;
        private readonly DriveService driveService;

        private readonly GoogleCredential credential;
        private readonly string[] scopes = {
            SheetsService.Scope.Spreadsheets,
            DriveService.Scope.Drive,
            DriveService.Scope.DriveReadonly,
            DriveService.Scope.DriveFile,
            DriveService.Scope.DriveMetadata
        };

        public GoogleApiManager(IOptions<AppSetting> appSetting)
        {
            var privateKey = appSetting.Value.CredentialsJson.PrivateKey
                            .Replace("\\n", "") // Remove escaped newline at the end of the BEGIN line
                            .Replace("\\r", ""); // Remove escaped carriage return at the end of the BEGIN line (if any)

            JsonCredentialParameters jsonCredential = new()
            {
                Type = appSetting.Value.CredentialsJson.Type,
                ProjectId = appSetting.Value.CredentialsJson.ProjectId,
                PrivateKeyId = appSetting.Value.CredentialsJson.PrivateKeyId.Decrypt(),
                PrivateKey = privateKey,
                ClientEmail = appSetting.Value.CredentialsJson.ClientEmail,
                ClientId = appSetting.Value.CredentialsJson.ClientId,
                TokenUri = appSetting.Value.CredentialsJson.TokenUri,

            };

            credential = GoogleCredential.FromJsonParameters(jsonCredential).CreateScoped(scopes);
            sheetsService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Academy",
            });

            driveService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Academy",
            });
        }

        public GoogleApiManager(string credentialsJson)
        {
            credential = GoogleCredential.FromJson(credentialsJson);
            sheetsService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Academy",
            });

            driveService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Academy",
            });
        }

        public async Task<IDictionary<string, string>> GetFileByFileId(string fileId)
        {
            var fileRequest = driveService.Files.Get(fileId);
            fileRequest.Fields = "*";
            var fileResponse = await fileRequest.ExecuteAsync();
            return fileResponse.AppProperties;
        }

        public async Task<Dictionary<string, IList<IList<object>>>> ReadRawDataFromWorksheetBySheetName(string worksheetId, string sheetrange)
        {
            Dictionary<string, IList<IList<object>>> spreadSheetData = [];
            string[] sheets = sheetrange.Trim(' ').Split(',');
            for (int i = 0; i <= sheets.Length - 1; i++)
            {
                SpreadsheetsResource.ValuesResource.GetRequest getRequest = sheetsService.Spreadsheets.Values.Get(worksheetId, sheets[i]);
                ValueRange response = await getRequest.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                spreadSheetData.Add(sheets[i], values);
            }
            return spreadSheetData;
        }

        public async Task<string> CreateFileOnDrive(string name, string type)
        {
            File FileMetaData = new()
            {
                Name = name,
                MimeType = type
            };

            FilesResource.CreateRequest request = driveService.Files.Create(FileMetaData);
            request.Fields = "id";
            var file = await request.ExecuteAsync();
            return file.Id;
        }
        public async Task<string> ReadFileContent(string fileId)
        {
            var request = driveService.Files.Get(fileId);
            MemoryStream stream = new();

            // Download the file content
            await request.DownloadAsync(stream);
            stream.Position = 0; // Reset the stream position

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        public async Task<string> UploadFileOnDrive(string name, string type, Stream fileContent)
        {
            File FileMetaData = new()
            {
                Name = name,
                MimeType = type
            };
            FilesResource.CreateMediaUpload request = driveService.Files.Create(FileMetaData, fileContent, type);
            request.Fields = "id";
            IUploadProgress file = await request.UploadAsync();
            return request.ResponseBody.Id;
        }

        public async Task<Dictionary<string, string>> ListAll(string folderId)
        {
            Dictionary<string, string> result = [];
            try
            {
                // Initial validation.
                if (driveService == null)
                    throw new("driveService");

                // Building the initial request.
                var request = driveService.Files.List();

                // Applying optional parameters to the request.                
                request.Q = $"mimeType != 'application/vnd.google-apps.folder' and '{folderId}' in parents";
                request.Fields = "nextPageToken, files(id, name, size, version, trashed, createdTime)";

                var pageStreamer = new Google.Apis.Requests.PageStreamer<Google.Apis.Drive.v3.Data.File, FilesResource.ListRequest, FileList, string>(
                                                   (req, token) => request.PageToken = token,
                                                   response => response.NextPageToken,
                                                   response => response.Files);


                FileList allFiles = new()
                {
                    Files = []
                };
                allFiles = await request.ExecuteAsync();

                foreach (File file in allFiles.Files)
                {
                    result.Add(file.Id, file.Name);
                }

                return result;

            }
            catch (Exception Ex)
            {
                throw new Exception("Request Files.List failed.", Ex);
            }
        }
        public async Task InsertFormulaByRange(string workbookId, string sheetName, IList<IList<object>> values, string range)
        {
            var _range = $"{sheetName}!{range}";
            var request = sheetsService.Spreadsheets.Values.Update(new ValueRange { Values = values }, workbookId, _range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var response = await request.ExecuteAsync();
        }

        private async Task<Spreadsheet> Worksheet(string worksheetId)
        {
            return await sheetsService.Spreadsheets.Get(worksheetId).ExecuteAsync();
        }

        public async Task<IList<IList<object>>> Insert(IList<IList<object>> values, string worksheetId, string range)
        {
            var request = sheetsService.Spreadsheets.Values.Append(new ValueRange { Values = values }, worksheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var response = await request.ExecuteAsync();
            return response.Updates.UpdatedData.Values;
        }

        public async Task UpdateRow(IList<IList<object>> values, string worksheetId, string range)
        {
            var request = sheetsService.Spreadsheets.Values.Update(new ValueRange { Values = values }, worksheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var response = await request.ExecuteAsync();
        }

        public async Task AppendRow(IList<IList<object>> values, string worksheetId, string range)
        {
            var request = sheetsService.Spreadsheets.Values.Append(new ValueRange { Values = values }, worksheetId, range);
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            var response = await request.ExecuteAsync();
        }

        public async Task<IList<IList<object>>> Insert(string value, string worksheetId, string range)
        {
            IList<IList<object>> values = new List<IList<object>>() { new List<object> { value } };
            var response = await Insert(values, worksheetId, range);
            return response;
        }

        public async Task<Dictionary<int?, string>> GetSheetsInWorksheet(string worksheetId)
        {
            var worksheet = await Worksheet(worksheetId);
            var sheets = worksheet.Sheets.ToDictionary(key => key.Properties.SheetId, value => value.Properties.Title);
            return sheets;
        }
        public async Task<bool> InsertRowAt(string worksheetId, int sheetId, int rowIndex)
        {
            DimensionRange dimension = new()
            {
                SheetId = sheetId,
                Dimension = "ROWS",
                StartIndex = rowIndex - 1,
                EndIndex = rowIndex
            };
            BatchUpdateSpreadsheetRequest request = new()
            {
                Requests =
                [
                    new(){InsertDimension = new InsertDimensionRequest(){ Range = dimension }},
                    new Request(){AutoResizeDimensions = new AutoResizeDimensionsRequest(){ Dimensions = dimension }}
                ]
            };
            await ExecuteBatch(request, worksheetId);
            return true;
        }
        public async Task<bool> InsertRowAt(string worksheetId, string sheetName, int rowIndex)
        {
            Spreadsheet spr = await Worksheet(worksheetId);
            Sheet sheet1 = spr.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            return await InsertRowAt(worksheetId, sheet1.Properties.SheetId.Value, rowIndex);
        }
        public async Task<string> MoveFileToAnotherFolder(string resourceId, string folderId)
        {
            FilesResource.GetRequest get_request = driveService.Files.Get(resourceId);
            get_request.Fields = "parents";
            File file = await get_request.ExecuteAsync();
            string previousParents = string.Join(",", file.Parents);

            // Move the file to the new folder   
            FilesResource.UpdateRequest update_request = driveService.Files.Update(new File(), resourceId);
            update_request.Fields = "id, parents";
            update_request.AddParents = folderId;
            update_request.RemoveParents = previousParents;
            update_request.SupportsAllDrives = true;
            file = await update_request.ExecuteAsync();
            return file.Id;
        }

        public async Task InsertColumnAt(string worksheetId, int sheetId, int columnIndex)
        {
            var dimension = new DimensionRange()
            {
                SheetId = sheetId,
                Dimension = "COLUMNS",
                StartIndex = columnIndex - 1,
                EndIndex = columnIndex
            };
            BatchUpdateSpreadsheetRequest request = new()
            {
                Requests =
                [
                    new(){InsertDimension = new InsertDimensionRequest(){ Range = dimension }},
                    new(){AutoResizeDimensions = new AutoResizeDimensionsRequest(){ Dimensions = dimension }}
                ]
            };
            await ExecuteBatch(request, worksheetId);
        }
        public async Task ClearData(string worksheetId, int sheetId)
        {
            BatchUpdateSpreadsheetRequest batch = new();
            Request clear_request = new()
            {
                UpdateCells = new()
                {
                    Range = new()
                    {
                        SheetId = sheetId
                    },
                    Fields = "*"
                }
            };
            batch.Requests = [clear_request];
            var batch_add = sheetsService.Spreadsheets.BatchUpdate(batch, worksheetId);
            var response = await batch_add.ExecuteAsync();
        }
        public async Task<int?> AddNewEmptySheetAsync(string worksheetId, string sheetName)
        {
            Spreadsheet worksheet = await Worksheet(worksheetId); ;
            Sheet sheet = worksheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            if (sheet != null)
            {
                return sheet.Properties.SheetId;
            }
            BatchUpdateSpreadsheetRequest add_batch = new();
            Request add_request = new()
            {
                AddSheet = new()
                {
                    Properties = new()
                    {
                        Title = sheetName
                    }
                }
            };
            add_batch.Requests = [add_request];
            var batch_add = sheetsService.Spreadsheets.BatchUpdate(add_batch, worksheetId);
            var response = await batch_add.ExecuteAsync();
            return response.Replies[0].AddSheet.Properties.SheetId;
        }
        public async Task<List<Tuple<int?, string, int>>> GetLastRowIndex(string worksheetId)
        {
            List<Tuple<int?, string, int>> response = [];
            var worksheet = await Worksheet(worksheetId);
            foreach (var sheet in worksheet.Sheets)
            {
                var sheet_data = await ReadRawDataFromWorksheetBySheetName(worksheetId, sheet.Properties.Title);
                int count = sheet_data.Values.FirstOrDefault() == null ? 0 : sheet_data.Values.FirstOrDefault().Count;
                response.Add(item: Tuple.Create<int?, string, int>(sheet.Properties.SheetId, sheet.Properties.Title, count));
            }
            return response;
        }

        public async Task MergeColumns(string worksheetId, string sheetName, int row, int fromColumn, int toColumn)
        {
            BatchUpdateSpreadsheetRequest request = await PrepareMergeCellRequest(worksheetId, sheetName, "MERGE_COLUMNS", row, row + 1, fromColumn, toColumn);
            var result = await ExecuteBatch(request, worksheetId);
        }
        public async Task MergeRows(string worksheetId, string sheetName, int column, int fromRow, int toRow)
        {
            BatchUpdateSpreadsheetRequest request = await PrepareMergeCellRequest(worksheetId, sheetName, "MERGE_ROWS", fromRow, toRow, column, column + 1);
            var result = await ExecuteBatch(request, worksheetId);
        }
        private async Task<BatchUpdateSpreadsheetRequest> PrepareMergeCellRequest(string worksheetId, string sheetName, string mergeType, int startRow, int endRow, int startCol, int endCol)
        {
            Spreadsheet spr = await Worksheet(worksheetId);
            Sheet sheet1 = spr.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            BatchUpdateSpreadsheetRequest request = new();
            Request merge_request = new()
            {
                MergeCells = new MergeCellsRequest()
                {
                    Range = new()
                    {
                        SheetId = (int)sheet1.Properties.SheetId,
                        StartColumnIndex = startCol,
                        EndColumnIndex = endCol,
                        StartRowIndex = startRow,
                        EndRowIndex = endRow,
                    },
                    MergeType = mergeType
                }
            };
            request.Requests = new List<Request>() { merge_request };
            return request;
        }


        public async Task RemoveSheetFromWorksheet(string worksheetId, string steetNameThatNeedsTobeRemoved)
        {
            Spreadsheet spr = await Worksheet(worksheetId);
            Sheet sheet1 = spr.Sheets.FirstOrDefault(s => s.Properties.Title == steetNameThatNeedsTobeRemoved);
            if (sheet1 != null)
            {
                BatchUpdateSpreadsheetRequest del_batch = new();
                Request delete_request = new()
                {
                    DeleteSheet = new()
                    {
                        SheetId = (int)sheet1.Properties.SheetId
                    }
                };
                del_batch.Requests = [delete_request];
                await ExecuteBatch(del_batch, worksheetId);
            }
        }

        public async Task<int> CopySingleSheetFromOneWorksheetToAnother(string sourceWorksheetId, string targetWorksheetId, string sourceSheetName)
        {
            CopySheetToAnotherSpreadsheetRequest copyRequestBody = new()
            {
                DestinationSpreadsheetId = targetWorksheetId
            };
            Spreadsheet sourceSpreadsheet = await Worksheet(sourceWorksheetId);
            var sheet = sourceSpreadsheet.Sheets.FirstOrDefault(x => x.Properties.Title == sourceSheetName);
            if (sheet != null)
            {
                SpreadsheetsResource.SheetsResource.CopyToRequest copyToRequest = null;

                copyToRequest = sheetsService.Spreadsheets.Sheets.CopyTo(copyRequestBody, sourceSpreadsheet.SpreadsheetId, sheet.Properties.SheetId.GetValueOrDefault());

                if (copyToRequest != null)
                {
                    SheetProperties copyResponse = await copyToRequest.ExecuteAsync();
                    return copyResponse.SheetId.Value;
                }
            }
            return 0;
        }


        public async Task CopyAllSheetFromOneWorksheetToAnotherExcept(string sourceWorksheetId, string targetWorksheetId, List<string> sheetNameToExclude)
        {
            CopySheetToAnotherSpreadsheetRequest copyRequestBody = new()
            {
                DestinationSpreadsheetId = targetWorksheetId
            };
            Spreadsheet sourceSpreadsheet = await Worksheet(sourceWorksheetId);
            foreach (var sheet in sourceSpreadsheet.Sheets)
            {
                SpreadsheetsResource.SheetsResource.CopyToRequest copyToRequest = null;
                if (sheetNameToExclude.Contains(sheet.Properties.Title))
                {
                    continue;
                }
                copyToRequest = sheetsService.Spreadsheets.Sheets.CopyTo(copyRequestBody, sourceSpreadsheet.SpreadsheetId, sheet.Properties.SheetId.GetValueOrDefault());

                if (copyToRequest != null)
                {
                    SheetProperties copyResponse = await copyToRequest.ExecuteAsync();
                }
            }
        }
        public async Task<KeyValuePair<string, string>> CreateNewWorksheet(string title)
        {
            Spreadsheet worksheet = new()
            {
                Properties = new SpreadsheetProperties()
                {
                    Title = title
                }
            };
            var response = await sheetsService.Spreadsheets.Create(worksheet).ExecuteAsync();
            return new KeyValuePair<string, string>(response.SpreadsheetId, response.SpreadsheetUrl);
        }

        public async Task<bool> DeleteColumn(string worksheetId, int sheetId, int columnIndex)
        {
            var requests = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = sheetId,
                        Dimension = "COLUMNS",
                        StartIndex = columnIndex - 1,
                        EndIndex = columnIndex
                    }
                }
            };
            BatchUpdateSpreadsheetRequest request = new()
            {
                Requests = new List<Request> { requests }
            };
            var response = await ExecuteBatch(request, worksheetId);
            return true;
        }

        public async Task<bool> DeleteRow(string worksheetId, int sheetId, int rowIndex)
        {
            var requests = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = sheetId,
                        Dimension = "ROWS",
                        StartIndex = rowIndex - 1,
                        EndIndex = rowIndex
                    }
                }
            };
            BatchUpdateSpreadsheetRequest request = new()
            {
                Requests = new List<Request> { requests }
            };
            var response = await ExecuteBatch(request, worksheetId);
            return true;
        }

        public async Task<bool> DeleteColumn(string worksheetId, string sheetName, int columnIndex)
        {
            Spreadsheet spr = await Worksheet(worksheetId);
            Sheet sheet1 = spr.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            return await DeleteColumn(worksheetId, sheet1.Properties.SheetId.Value, columnIndex);
        }

        public async Task<bool> DeleteRow(string worksheetId, string sheetName, int rowIndex)
        {
            Spreadsheet spr = await Worksheet(worksheetId);
            Sheet sheet1 = spr.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            return await DeleteRow(worksheetId, sheet1.Properties.SheetId.Value, rowIndex);
        }

        public async Task CreateDropdownCell(string worksheetId, int? sheetID, int startRowIndex,
            int StartColumnIndex, int endColumnIndex,
            params string[] values)
        {
            var valuesList = new List<ConditionValue>();
            foreach (var val in values)
            {
                valuesList.Add(new ConditionValue() { UserEnteredValue = val });
            }
            var updateCellsRequest = new Request()
            {
                SetDataValidation = new SetDataValidationRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheetID,
                        StartRowIndex = startRowIndex,
                        StartColumnIndex = StartColumnIndex,
                        EndColumnIndex = endColumnIndex
                    },
                    Rule = new DataValidationRule()
                    {
                        Condition = new BooleanCondition()
                        {
                            Type = "ONE_OF_LIST",
                            Values = valuesList
                        },
                        ShowCustomUi = true,
                        Strict = true,
                    }
                },
            };
            var requestBody = new BatchUpdateSpreadsheetRequest();
            var requests = new List<Request>() { updateCellsRequest };
            requestBody.Requests = requests;
            var batchRequest = sheetsService.Spreadsheets.BatchUpdate(requestBody, worksheetId);
            var response = await batchRequest.ExecuteAsync();
        }

        public async Task<bool> RenameSheet(string worksheetId, int sheetIdWhoseNameNeedsToBeChanged, string newSheetName)
        {
            var request = new Request()
            {
                UpdateSheetProperties = new UpdateSheetPropertiesRequest
                {
                    Properties = new SheetProperties()
                    {
                        Title = newSheetName,
                        SheetId = sheetIdWhoseNameNeedsToBeChanged
                    },
                    Fields = "Title"
                }
            };
            BatchUpdateSpreadsheetRequest batch = new()
            {
                Requests = new List<Request> { request }
            };
            var response = await ExecuteBatch(batch, worksheetId);
            return true;
        }
        public async Task<bool> MoveSheetPosition(string worksheetId, int sheetId, int newPosition)
        {
            var request = new Request()
            {
                UpdateSheetProperties = new UpdateSheetPropertiesRequest
                {
                    Properties = new SheetProperties()
                    {
                        SheetId = sheetId,
                        Index = newPosition
                    },
                    Fields = "Index"
                }
            };
            BatchUpdateSpreadsheetRequest batch = new()
            {
                Requests = new List<Request> { request }
            };
            var response = await ExecuteBatch(batch, worksheetId);
            return true;
        }



        public async Task<bool> SetCellBackgroundAndForeGroundColor(string worksheetId, int sheetId, int startRowIndex, int endRowIndex, int startColumnIndex, int endColumnIndex,
            int backgroundColorBlue, int backgroundColorGreen, int backgroundColorRed, int fontColorBlue, int fontColorGreem, int fontColorRed,
            bool textBold = false, bool horizontalAlignmentCenter = false)
        {
            var request = new Request()
            {
                RepeatCell = new RepeatCellRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartRowIndex = startRowIndex,
                        EndRowIndex = endRowIndex,
                        StartColumnIndex = startColumnIndex,
                        EndColumnIndex = endColumnIndex
                    },
                    Cell = new CellData()
                    {
                        UserEnteredFormat = new CellFormat()
                        {
                            BackgroundColor = new Color()
                            {
                                Blue = backgroundColorBlue,
                                Green = backgroundColorGreen,
                                Red = backgroundColorRed
                            },
                            TextFormat = new TextFormat()
                            {
                                ForegroundColor = new Color()
                                {
                                    Blue = fontColorBlue,
                                    Green = fontColorGreem,
                                    Red = fontColorRed
                                },
                                Bold = textBold
                            },
                            HorizontalAlignment = (horizontalAlignmentCenter == true) ? "CENTER" : "LEFT",
                            VerticalAlignment = "MIDDLE"
                        }
                    },
                    Fields = "UserEnteredFormat(BackgroundColor,TextFormat)"
                }
            };
            BatchUpdateSpreadsheetRequest batch = new()
            {
                Requests = new List<Request> { request }
            };
            var response = await ExecuteBatch(batch, worksheetId);
            return true;
        }
        public async Task GrantPermissionTo(string resourceId, params string[] candidateEmails)
        {
            PermissionsResource.CreateRequest request = default;
            foreach (var candidateEmail in candidateEmails)
            {
                Permission permission = new()
                {
                    Role = "writer",
                    Type = "user",
                    EmailAddress = candidateEmail.Trim()
                };
                request = driveService.Permissions.Create(
                    permission,
                    resourceId
                    );
                request.SendNotificationEmail = false;
                request.SupportsAllDrives = true;
                await request.ExecuteAsync();
            }
         
        }


        public async Task<string> RenameFile(string resourceId, string newName)
        {
            File file = new() { Name = newName };
            var updateRequest = driveService.Files.Update(file, resourceId);
            updateRequest.Fields = "name";
            file = await updateRequest.ExecuteAsync();
            return file.Id;
        }

        //public async Task<SearchDirectoryPeopleResponse> GetPeopleDetail(string requestParameter, CancellationToken cancellationToken = default)
        //{
        //    await Task.Delay(100, cancellationToken);
        //    throw new NotImplementedException();
        //    //var peopleRequest = peopleService.People.SearchDirectoryPeople();
        //    //peopleRequest.Query = requestParameter.ToLower();
        //    //peopleRequest.ReadMask = "names,emailAddresses,photos";
        //    //peopleRequest.MergeSources = SearchDirectoryPeopleRequest.MergeSourcesEnum.DIRECTORYMERGESOURCETYPECONTACT;
        //    //peopleRequest.Sources = SearchDirectoryPeopleRequest.SourcesEnum.DIRECTORYSOURCETYPEDOMAINPROFILE;
        //    //
        //    //return await peopleRequest.ExecuteAsync(cancellationToken);
        //}
        private async Task<BatchUpdateSpreadsheetResponse> ExecuteBatch(BatchUpdateSpreadsheetRequest request, string worksheetId)
        {
            return await sheetsService.Spreadsheets.BatchUpdate(request, worksheetId).ExecuteAsync();
        }


        public async Task<string> WriteSheetDirectly(string spreadsheetId,DataTable table,string sheetName)
        {
            if (string.IsNullOrEmpty(spreadsheetId)) throw new ArgumentNullException(nameof(spreadsheetId));
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrEmpty(sheetName)) throw new ArgumentNullException(nameof(sheetName));
            if (sheetsService == null) throw new ArgumentNullException(nameof(sheetsService));

            // Escape single quotes in sheet name for A1 range usage
            var escapedSheetName = sheetName.Replace("'", "''");
            var rangeA1 = $"'{escapedSheetName}'!A1";

            try
            {
                // 1) Ensure sheet exists; if not, create it.
                var spreadsheet = await sheetsService.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
                bool sheetExists = spreadsheet.Sheets?.Any(s => string.Equals(s.Properties?.Title, sheetName, StringComparison.OrdinalIgnoreCase)) ?? false;
                int? sheetId = 0;

                if (!sheetExists)
                {
                    var addSheetRequest = new Request
                    {
                        AddSheet = new AddSheetRequest
                        {
                            Properties = new SheetProperties { Title = sheetName }
                        }
                    };

                    var batchUpdateRequest = new BatchUpdateSpreadsheetRequest { Requests = new List<Request> { addSheetRequest } };
                    try
                    {
                        var response = await sheetsService.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetId).ExecuteAsync();
                        sheetId = response.Replies[0].AddSheet.Properties.SheetId;
                    }
                    catch (Google.GoogleApiException gae)
                    {
                        // If another process created the sheet simultaneously, ignore "already exists" errors.
                        // Otherwise rethrow.
                        // You can inspect gae.Error to be more specific if desired.
                        // For simplicity, swallow and re-check existence:
                        spreadsheet = await sheetsService.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
                    }
                }

                // 2) Convert DataTable to IList<IList<object>>
                IList<IList<object>> ToValues(DataTable dt)
                {
                    var rows = new List<IList<object>>();
                    // header
                    rows.Add(dt.Columns.Cast<DataColumn>().Select(c => (object)c.ColumnName).ToList());
                    // data
                    foreach (DataRow r in dt.Rows)
                    {
                        rows.Add(r.ItemArray.Select(x => (object)(x ?? string.Empty)).ToList());
                    }
                    return rows;
                }

                var values = ToValues(table);

                // 3) Prepare ValueRange and call Update (start at A1; Sheets will expand to fit)
                var valueRange = new ValueRange
                {
                    Range = rangeA1,
                    Values = values
                };

                var updateRequest = sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, valueRange.Range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var updateResponse = await updateRequest.ExecuteAsync();

                // Get start and end row indices for the first column
                int startColIndex = 0; // Starting from the first row
                int endRowIndex = table.Columns.Count; // +1 to account for the header row

                await SetCellBackgroundAndForeGroundColor(spreadsheetId, sheetId ?? 0, 0, 1, startColIndex, endRowIndex, 0, 1, 1, 0, 0, 0, true, true);

                // 4) Return sheet URL
                return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={GetSheetGid(spreadsheet, sheetName)}";
            }
            catch (Google.GoogleApiException gex)
            {
                // bubble or log accordingly
                throw;
            }
        }

        // Helper to get the gid for the named sheet (used to return a link directly to that tab).
        private int GetSheetGid(Spreadsheet spreadsheet, string sheetName)
        {
            var s = spreadsheet.Sheets?.FirstOrDefault(sh => string.Equals(sh.Properties?.Title, sheetName, StringComparison.OrdinalIgnoreCase));
            return s?.Properties?.SheetId ?? 0;
        }

        public async Task<Dictionary<string, string>> ListAllChildFolders(string folderId)
        {
            var request = driveService.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false";
            request.Fields = "files(id, name, mimeType, size, webViewLink)";
            var result = request.Execute().Files;
            Dictionary<string, string> filesInfo = new();

            foreach (var file in result) 
            {
                filesInfo.Add(file.Name, file.Id);
            }

            return filesInfo;
        }

        public async Task<string> CreateFolder(string folderName, string parentFolderId = null)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            if (!string.IsNullOrEmpty(parentFolderId))
            {
                fileMetadata.Parents = new List<string> { parentFolderId };
            }
            var request = driveService.Files.Create(fileMetadata);
            request.Fields = "id";
            var folder = await request.ExecuteAsync();
            return folder.Id;
        }

        public async Task<(DateTime modifiedOn, string webContentLink)> UploadFile(IFormFile file, string fileName, string parentFolderId = null)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = new[] { parentFolderId }
            };

            using var fileStream = file.OpenReadStream();
            var createRequest = driveService.Files.Create(fileMetadata, fileStream, file.ContentType);
            createRequest.Fields = "id, name, parents, webViewLink, webContentLink";
            createRequest.SupportsAllDrives = true;

            IUploadProgress progress = await createRequest.UploadAsync();

            if (progress.Status == UploadStatus.Completed)
            {
                var id = createRequest.ResponseBody.Id;

                var permission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Role = "reader",
                    Type = "anyone"
                };
                await driveService.Permissions.Create(permission, id).ExecuteAsync();


                var _file = driveService.Files.Get(id);
                _file.Fields = "id, name, modifiedTime";
                var fileDetails = await _file.ExecuteAsync();

                return (fileDetails?.ModifiedTime ?? DateTime.Now, createRequest.ResponseBody.WebContentLink);
            }

            return (DateTime.Now, string.Empty);
        }

        public async Task DeleteFile(string fileId)
        {
            await driveService.Files.Delete(fileId).ExecuteAsync();
        }
    }
}
