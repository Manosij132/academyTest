using Academy.Shared.Constants;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using Staffing.Core.Abstraction.Services;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Staffing.API.Helpers
{
    public class AppSettingsResolver
    {
        private readonly IConfiguration _configuration;
        private readonly string _environment;
        private readonly string _connectionString;
        private readonly IAISettingsProvider _settingsProvider;
        private readonly AIConnection aiConnection;
        public AppSettingsResolver(IConfiguration configuration, IAISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
             aiConnection = _settingsProvider.GetAIConnection();
            _configuration = configuration;
            _environment = _configuration["Environment"] ?? "dev";           
        }

        public async Task ResolveAsync(object settings)
        {
            if (settings == null)
                return;

            var properties = settings.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(settings);

                // Handle collections
                if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) &&
                    property.PropertyType != typeof(string))
                {
                    if (value is IEnumerable collection)
                    {
                        foreach (var item in collection)
                        {
                            await ResolveAsync(item);
                        }
                    }
                }
                // Handle nested classes
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    await ResolveAsync(value);
                }
                // Resolve ##KEY##
                else if (value is string strValue &&
                         strValue.StartsWith("##") &&
                         strValue.EndsWith("##"))
                {
                    var keyName = strValue.Trim('#');
                    var dbValue = await FetchFromDatabaseAsync(keyName);
                    property.SetValue(settings, dbValue);
                }
            }
        }

        private async Task<string> FetchFromDatabaseAsync(string keyName)
        {
            const string query = @"
            SELECT [Value]
            FROM Configurations
            WHERE LOWER(Environment) = LOWER(@Environment)
              AND LOWER([Key]) = LOWER(@Key)";

            await using var connection = new SqlConnection(aiConnection.StaffingDbConnection.ConnectionString);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@Environment", SqlDbType.NVarChar).Value = _environment;
            command.Parameters.Add("@Key", SqlDbType.NVarChar).Value = keyName;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            if (result != null)
            {
                return result.ToString();
            }

            throw new Exception(Messages.ERROR_CONFIG_KEY_NOT_FOUND);
        }
    }
}
