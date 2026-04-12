using Academy.Core.Abstraction.Infrastructure;
using Academy.Core.Models;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Academy.Shared.DTO.DBSchema;
using Academy.Shared.Extensions;
namespace Academy.Infrastructure.AdoClient
  
{
    public class SqlServer<T> : BaseAdo, IAdoClient<T>, ISchemaInspector where T : IAdoSetting
    {
        private const string dbType = "SqlServer";

        public SqlServer(IAdoSetting adoSetting)
        {
            if (string.IsNullOrWhiteSpace(adoSetting.ConnectionString))
                throw new ArgumentNullException("ConnectionString is empty");
            _connectionString = adoSetting.ConnectionString;
            if (!_connectionFactory.ContainsKey(nameof(SqlServer<T>)))
            {
                _connectionFactory.Add(nameof(SqlServer<T>), new Lazy<DbConnection>(() => new SqlConnection()));
            }
        }
        protected override DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        protected override DbCommand CreateCommand(string procedureName, Dictionary<string, object> inParams)
        {
            var command = new SqlCommand
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

        public async Task<List<List<string>>> ExecuteQueryAsListAsync(string sqlQuery)
        {
            var rows = new List<List<string>>();

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sqlQuery, connection);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
                return rows;

            // Add headers
            var headerRow = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                headerRow.Add(reader.GetName(i));
            }
            rows.Add(headerRow);

            // Add data rows
            while (await reader.ReadAsync())
            {
                var row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        row.Add(reader.GetValue(i)?.ToFormattedString() ?? "NULL");
                    }
                    catch
                    {
                        row.Add("DataTypeConversionError");
                    }
                }
                rows.Add(row);
            }

            return rows;
        }
       
        public async Task<DatabaseSchema> GenerateSchemaAsync()
        {
            const string sql = @"
        SELECT SCHEMA_NAME(schema_id) + '.' + o.Name AS TableName,
               c.Name AS ColumnName
        FROM sys.columns c
        JOIN sys.objects o ON o.object_id = c.object_id
        WHERE o.type = 'U'
        ORDER BY o.Name";

            var rows = await ExecuteQueryAsListAsync(sql);

            var dbSchema = new DatabaseSchema
            {
                SchemaRaw = new List<string>(),
                SchemaStructured = new List<TableSchema>()
            };

            if (rows.Count <= 1) // No data rows
                return dbSchema;

            // Group by table name (skip header row)
            var grouped = rows.Skip(1).GroupBy(r => r[0]);

            foreach (var group in grouped)
            {
                dbSchema.SchemaStructured.Add(new TableSchema
                {
                    TableName = group.Key,
                    Columns = group.Select(r => r[1]).ToList()
                });
            }

            // Raw string representation
            dbSchema.SchemaRaw = dbSchema.SchemaStructured
                .Select(t => $"- {t.TableName} ({string.Join(", ", t.Columns)})")
                .ToList();

            return dbSchema;
        }
        public async Task<List<Dictionary<string, string>>> ExecuteQueryAsJsonListAsync(string sqlQuery)
        {
            var resultList = new List<Dictionary<string, string>>();

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sqlQuery, connection);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
                return resultList;

            var headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                headers.Add(reader.GetName(i));
            }

            while (await reader.ReadAsync())
            {
                var rowDict = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        var value = reader.GetValue(i)?.ToFormattedString() ?? "NULL";
                        rowDict[headers[i]] = value;
                    }
                    catch
                    {
                        rowDict[headers[i]] = "DataTypeConversionError";
                    }
                }

                resultList.Add(rowDict);
            }

            return resultList;
        }

        protected override void InitializeCommand(string procedureName, Dictionary<string, object> inParameters)
        {
            _command = new SqlCommand
            {
                CommandText = procedureName,
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Clear();
            if (inParameters != null)
            {
                foreach (var item in inParameters)
                {
                    if (item.Key.StartsWith("@"))
                        ((SqlCommand)_command).Parameters.AddWithValue(item.Key, item.Value);
                    else
                        ((SqlCommand)_command).Parameters.AddWithValue($"@{item.Key}", item.Value);
                }
            }
        }

        protected override DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command); // Cast to SqlCommand
        }
    }
}
