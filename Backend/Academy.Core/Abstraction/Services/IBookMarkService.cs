using Academy.Shared.DTO;
using System.Data;

namespace Academy.Core.Abstraction.Services
{
    public interface IBookMarkService
    {
        Task<AcademyResponse<BookMarkTemplateListDto>> Insert(BookMarkRequest request);
        List<BookMarkTemplateListDto> Fetch();
        Task<AcademyResponse<BookMarkTemplateListDto>> Modify(BookMarkRequest request);
        Task<string> Deactivate(int bookMarkId);
        BookMarkTemplatesDto Search(int bookMarkId);
        Task<dynamic> GetReportData(BookMarkRequest request , bool fromExport= false);
        Task<string> SendReportData(ReportEmailRequest reportEmailRequest);
        Task<string> GenerateReportData(int BookMarkId);
        DataTable ConvertJsonToDataTable(string jsonData);
        string ConvertDataTableToHTML1(DataTable dt);
        Task<string> ReplaceTable(string GeneratedHTMLTable, string headerMessage = "", string reportName = "");

        Task<string> ExportGenerateReportDataBookMarkRequest(BookMarkRequest bookMarkRequest);
    }
}