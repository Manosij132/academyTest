using Academy.Core.Models;
using System.Data;

namespace Academy.Core.Abstraction.Infrastructure
{
    public interface IAdoClient<T> where T : IAdoSetting
    {
        Task<object> ExecuteScalerAsync(string procedureName, Dictionary<string, object> inParameters);
        Task<DataTable> ExecuteReaderAsync(string procedureName, Dictionary<string, object> inParameters);
        Task<int> ExecuteNonQueryAsync(string procedureName, Dictionary<string, object> inParameters);
        Task<DataSet> XecuteReaderDataSetAsync(string procedureName, Dictionary<string, object> inParameters);
        Task<List<List<string>>> ExecuteQueryAsListAsync(string query);
        Task<List<Dictionary<string, string>>> ExecuteQueryAsJsonListAsync(string sqlQuery);
    }
}
