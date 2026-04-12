using Microsoft.Data.SqlClient;
using System.Data;

namespace Staffing.Core.Abstraction.Infrastructure.Helpers
{
    public static class SqlHelper
    {
        private const int DefaultCommandTimeout = 120;

        /// <summary>
        /// Creates a stored procedure command with default settings.
        /// </summary>
        public static SqlCommand CreateStoredProcedure(
            SqlConnection connection,
            string storedProcedureName,
            int? commandTimeout = null)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be null or empty.");

            var command = new SqlCommand(storedProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = commandTimeout ?? DefaultCommandTimeout
            };

            return command;
        }

        /// <summary>
        /// Adds a parameter with null handling.
        /// </summary>
        public static void AddParameter(
            SqlCommand command,
            string parameterName,
            SqlDbType dbType,
            object value)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var parameter = command.Parameters.Add(parameterName, dbType);
            parameter.Value = value ?? DBNull.Value;
        }
    }
}
