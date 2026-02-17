// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;
using System.Data.SqlClient;

namespace YouTubeShortAutomator.Data;

public class DatabaseContext : IAsyncDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private bool _disposed;

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<SqlConnection> GetConnectionAsync()
    {
        // Opens and returns a database connection
        if (_connection == null)
        {
            _connection = new SqlConnection(_connectionString);
            await _connection.OpenAsync();
        }
        return _connection;
    }

    public async Task<int> ExecuteCommandAsync(string commandText, CommandType commandType = CommandType.Text,
        Dictionary<string, object?>? parameters = null)
    {
        // Executes a SQL command with optional parameters
        try
        {
            var connection = await GetConnectionAsync();
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandType = commandType;
                command.CommandTimeout = 30;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException($"Database command execution failed: {ex.Message}", ex);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string commandText, CommandType commandType = CommandType.Text,
        Dictionary<string, object?>? parameters = null)
    {
        // Executes a command and returns a scalar value
        try
        {
            var connection = await GetConnectionAsync();
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandType = commandType;
                command.CommandTimeout = 30;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var result = await command.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? (T?)Convert.ChangeType(result, typeof(T)) : default;
            }
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException($"Database scalar query failed: {ex.Message}", ex);
        }
    }

    public async Task<DataTable> ExecuteDataTableAsync(string commandText, CommandType commandType = CommandType.Text,
        Dictionary<string, object?>? parameters = null)
    {
        // Executes a query and returns results as a DataTable
        try
        {
            var connection = await GetConnectionAsync();
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandType = commandType;
                command.CommandTimeout = 30;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    await Task.Run(() => adapter.Fill(dataTable));
                    return dataTable;
                }
            }
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException($"Database table query failed: {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Properly closes and disposes the database connection
        if (_disposed) return;

        if (_connection != null)
        {
            if (_connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
            _connection.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
