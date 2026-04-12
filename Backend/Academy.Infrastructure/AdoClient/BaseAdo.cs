using System.Data.Common;
using System.Data;

namespace Academy.Infrastructure.AdoClient
{
    public abstract class BaseAdo
    {
        #region Fields and Properties
        protected string _connectionString;
        protected Dictionary<string, Lazy<DbConnection>> _connectionFactory = new();
        protected DbCommand _command;
        protected DbConnection _connection;
        protected abstract DbConnection CreateConnection();
        protected abstract DbCommand CreateCommand(string procedureName, Dictionary<string, object> inParams);

        #endregion
        #region Private Methods
        private DbConnection GetConnection(string key)
        {
            if (_connectionFactory.TryGetValue(key, out Lazy<DbConnection> lazyFactory))
            {
                return lazyFactory.Value;
            }
            throw new InvalidOperationException("No factory registered for the given key.");
        }
        private async Task OpenAsync(string dbType)
        {
            _connection = GetConnection(dbType);
            _connection.ConnectionString = _connectionString;
            await _connection.OpenAsync();
        }
        private async Task CloseAsync()
        {
            if (_command != null)
                await _command.DisposeAsync();
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                await _connection.CloseAsync();
            }
        }
        #endregion
        protected abstract void InitializeCommand(string procedureName, Dictionary<string, object> inParameters);
        #region Execute Methods
        protected async Task<object> XecuteScalerAsync(string procedureName, Dictionary<string, object> inParameters, string dbType)
        {
                await using var connection = CreateConnection();
                await connection.OpenAsync();

                await using var command = CreateCommand(procedureName, inParameters);
                command.Connection = connection;


                if (command != null)
                {
                    object result = await command.ExecuteScalarAsync();
                    return result ?? new();
                }
            return new();
        }
        protected async Task<DataTable> XecuteReaderAsync(string procedureName, Dictionary<string, object> inParameters, string dbType)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateCommand(procedureName, inParameters);
            command.Connection = connection;


            if (command != null)
            {
                await using var reader = await command.ExecuteReaderAsync();
                DataTable resultSet = new();
                resultSet.Load(reader);
                return resultSet;
            }
            return new();
        }

        protected async Task<DataSet> XecuteReaderDataSetAsync(string procedureName, Dictionary<string, object> inParameters, string dbType)
        {
            DataSet dataSet = new DataSet();
            try
            {
                await OpenAsync(dbType);
                InitializeCommand(procedureName, inParameters);
                if (_command != null)
                {
                    _command.Connection = _connection;

                    // Use DbDataAdapter to fill the DataSet
                    using (DbDataAdapter adapter = CreateDataAdapter(_command))
                    {
                        await Task.Run(() => adapter.Fill(dataSet));
                    }
                }
            }
            catch
            {
                // Optionally log or handle the exception here
                throw;
            }
            finally
            {
                await CloseAsync();
            }

            return dataSet;
        }
        protected async Task<int> XecuteNonQueryAsync(string procedureName, Dictionary<string, object> inParameters, string dbType)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateCommand(procedureName, inParameters);
            command.Connection = connection;


            if (command != null)
            {
                int result = await command.ExecuteNonQueryAsync();
                return result;
            }
            return 0;
        }
        #endregion
        protected abstract DbDataAdapter CreateDataAdapter(DbCommand command);

    }
}