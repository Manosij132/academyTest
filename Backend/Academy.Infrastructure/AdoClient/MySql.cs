using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
namespace Academy.Infrastructure.AdoClient
{
    public class MySql<T> : BaseAdo, IAdoClient<T> where T : IAdoSetting
    {
        private const string dbType = "MySql";

        public MySql(IAdoSetting adoSetting)
        {
            if (string.IsNullOrWhiteSpace(adoSetting.ConnectionString))
                throw new ArgumentNullException("ConnectionString is empty");
            _connectionString = adoSetting.ConnectionString;
            if (!_connectionFactory.ContainsKey(nameof(MySql<T>)))
            {
                _connectionFactory.Add(nameof(MySql<T>), new Lazy<DbConnection>(() => new MySqlConnection()));
            }
        }
        protected override DbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString); // from MySql.Data.MySqlClient
        }

        protected override DbCommand CreateCommand(string procedureName, Dictionary<string, object> inParams)
        {
            var command = new MySqlCommand
            {
                CommandText = procedureName,
                CommandType = CommandType.StoredProcedure
            };

            if (inParams != null)
            {
                foreach (var item in inParams)
                {
                    var paramName = item.Key.StartsWith("@") ? item.Key : $"@{item.Key}";
                    command.Parameters.AddWithValue(paramName, item.Value ?? DBNull.Value);
                }
            }

            return command;
        }
        public async Task<object> ExecuteScalerAsync(string procedureName, Dictionary<string, object> inParameters)
        {
            return await XecuteScalerAsync(procedureName, inParameters, dbType);
        }
        public async Task<DataTable> ExecuteReaderAsync(string procedureName, Dictionary<string, object> inParameters)
        {
            return await XecuteReaderAsync(procedureName, inParameters, dbType);
        }
        public async Task<DataSet> XecuteReaderDataSetAsync(string procedureName, Dictionary<string, object> inParameters)
        {
            return await XecuteReaderDataSetAsync(procedureName, inParameters, dbType);
        }
        public async Task<int> ExecuteNonQueryAsync(string procedureName, Dictionary<string, object> inParameters)
        {
            return await XecuteNonQueryAsync(procedureName, inParameters, dbType);
        }

        public Task<List<List<string>>> ExecuteQueryAsListAsync(string query)
        {
            throw new NotImplementedException();
        }
        protected override void InitializeCommand(string procedureName, Dictionary<string, object> inParameters)
        {
            _command = new MySqlCommand();
            _command.CommandText = procedureName;
            _command.CommandType = CommandType.StoredProcedure;
            _command.Parameters.Clear();
            if (inParameters != null)
            {
                foreach (var item in inParameters)
                {
                    if (item.Key.StartsWith("@"))
                        ((MySqlCommand)_command).Parameters.AddWithValue(item.Key, item.Value);
                    else
                        ((MySqlCommand)_command).Parameters.AddWithValue($"@{item.Key}", item.Value);
                }
            }
        }

        protected override DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new MySqlDataAdapter((MySqlCommand)command); // Cast to SqlCommand
        }

        public Task<List<Dictionary<string, string>>> ExecuteQueryAsJsonListAsync(string sqlQuery)
        {
            throw new NotImplementedException();
        }
    }
}