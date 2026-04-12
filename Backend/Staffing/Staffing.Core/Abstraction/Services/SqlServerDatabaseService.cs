using Microsoft.Data.SqlClient;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Services
{
    public class SqlServerDatabaseService : IDatabaseService
    {
        public async Task<List<Dictionary<string, string>>> GetDataTable(DataConnection conn, string sqlQuery)
        {
            var rows = new List<Dictionary<string, string>>();

            using (SqlConnection connection = new SqlConnection(conn.ConnectionString))
            {
                using var command = new SqlCommand(sqlQuery, connection);
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        var rowDict = new Dictionary<string, string>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i).ToString();
                            try
                            {
                                var value = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                                var headerName = Regex.Replace(columnName, "([A-Z])", " $1").Trim();
                                rowDict[headerName] = value;
                            }
                            catch
                            {
                                rowDict[columnName] = "DataTypeConversionError";
                            }
                        }

                        rows.Add(rowDict);
                    }
                }
            }

            return rows;
        }

        //public async Task<List<List<string>>> GetDataTable(AIConnection conn, string sqlQuery)
        //{
        //    var rows = new List<List<string>>();
        //    using (SqlConnection connection = new SqlConnection(conn.ConnectionString))
        //    {
        //        using var command = new SqlCommand(sqlQuery, connection);

        //        await connection.OpenAsync();
        //        using var reader = await command.ExecuteReaderAsync();

        //        int count = 0;
        //        bool headersAdded = false;
        //        if (reader.HasRows){
        //            while (await reader.ReadAsync())
        //            {
        //                var cols = new List<string>();
        //                var headerCols = new List<string>();
        //                if (!headersAdded)
        //                {
        //                    for (int i = 0; i < reader.FieldCount; i++)
        //                    {
        //                        headerCols.Add(reader.GetName(i).ToString());
        //                    }
        //                    headersAdded = true;
        //                    rows.Add(headerCols);
        //                }

        //                for (int i = 0; i <= reader.FieldCount - 1; i++)
        //                {
        //                    try
        //                    {
        //                        cols.Add(reader.GetValue(i).ToString());
        //                    }
        //                    catch
        //                    {
        //                        cols.Add("DataTypeConversionError");
        //                    }
        //                }
        //                rows.Add(cols);
        //            }
        //        }
        //    }

        //    return rows;
        //}

        public async Task<DatabaseSchema> GenerateSchema(DataConnection conn)
        {
            var dbSchema = new DatabaseSchema() { SchemaRaw = new List<KeyValuePair<string, string>>(), SchemaStructured = new List<TableSchema>() };
            List<KeyValuePair<string, string>> rows = new();

            using (SqlConnection connection = new SqlConnection(conn.ConnectionString))
            {
                await connection.OpenAsync();

                //string sql = $@"SELECT SCHEMA_NAME(schema_id) + '.' + o.Name AS 'TableName', c.Name as 'ColumName'
                //FROM     sys.columns c
                //         JOIN sys.objects o ON o.object_id = c.object_id
                //WHERE    o.type = 'U' AND o.Name in ('{string.Join("','", conn.Entities)}')
                //ORDER BY o.Name";

                string sql = $@"SELECT '[' + TABLE_SCHEMA + '.' + TABLE_NAME + ']' AS 'TableName' ,
	                        '[' + COLUMN_NAME + '] ' + '[' + DATA_TYPE + ']' + CASE WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN '(' + CONVERT(varchar(100), CHARACTER_MAXIMUM_LENGTH) + ')' ELSE '' END + CASE WHEN IS_NULLABLE = 'yes' THEN ' NULL' ELSE ' NOT NULL' END as 'ColumName'
                            FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME IN ('{string.Join("','", conn.Entities)}')";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rows.Add(new KeyValuePair<string, string>(reader.GetValue(0).ToString(), reader.GetValue(1).ToString()));
                        }
                    }
                }
            }

            var groups = rows.GroupBy(x => x.Key);

            foreach (var group in groups)
            {
                dbSchema.SchemaStructured.Add(new TableSchema() { TableName = group.Key, Columns = group.Select(x => x.Value).ToList() });
                //use this list
            }

            var textLines = new List<KeyValuePair<string, string>>();

            foreach (var table in dbSchema.SchemaStructured)
            {
                var schemaLine = $"Here are the details of the  {table.TableName} table: (";

                foreach (var column in table.Columns)
                {
                    schemaLine += column + ", \\r\\n\\t";
                }

                schemaLine += ")";
                schemaLine = schemaLine.Replace(", )", " )");
                
                textLines.Add(new(table.TableName, schemaLine));
            }

            dbSchema.SchemaRaw = textLines;

            return dbSchema;
        }
    }
}
