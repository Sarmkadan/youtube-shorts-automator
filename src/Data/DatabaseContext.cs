// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;
using System.Data.SqlClient;
using YouTubeShortAutomator.Exceptions;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Represents a database context for interacting with the database.
/// </summary>
public class DatabaseContext : IAsyncDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string to use for the database context.</param>
    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Gets a database connection.
    /// </summary>
    /// <returns>A database connection.</returns>
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

    /// <summary>
    /// Executes a database command.
    /// </summary>
    /// <param name="commandText">The command text to execute.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="parameters">The parameters for the command.</param>
    /// <returns>The number of rows affected by the command.</returns>
    public async Task<int> ExecuteCommandAsync(string commandText, CommandType commandType = CommandType.Text,
        Dictionary<string, object?>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            throw new ArgumentException("Command text cannot be null or empty.", nameof(commandText));

        try
        {
            var connection = await GetConnectionAsync();
            using var command = new SqlCommand(commandText, connection)
            {
                CommandType = commandType,
                CommandTimeout = 30
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            return await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            throw new YoutubeShortsAutomatorException($"Database command execution failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new YoutubeShortsAutomatorException("Unexpected error during ExecuteCommandAsync.", ex);
        }
    }

    /// <summary>
    /// Executes a database scalar query.
    /// </summary>
    /// <param name="commandText">The command text to execute.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="parameters">The parameters for the command.</param>
    /// <returns>The result of the scalar query.</returns>
    public async Task<T?> ExecuteScalarAsync<T>(string commandText, CommandType commandType = CommandType.Text,
        Dictionary<string, object?>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            throw new ArgumentException("Command text cannot be null or empty.", nameof(commandText));

        try
        {
            var connection = await GetConnectionAsync();
            using var command = new SqlCommand(commandText, connection)
            {
                CommandType = commandType,
                CommandTimeout = 30
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            var result = await command.ExecuteScalarAsync();
            return result != null && result != DBNull.Value
                ? (T?)Convert.ChangeType(result, typeof(T))
                : default;
        }
        catch (SqlException ex)
        {
            throw new YoutubeShortsAutomatorException($"Database scalar query failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new YoutubeShortsAutomatorException("Unexpected error during ExecuteScalarAsync.", ex);
        }
    }

    /// <summary>
    /// Executes a database table query.
    /// </summary>
    /// <param name="commandText">The command text to execute.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="parameters">The parameters for the command.</param>
    /// <returns>A DataTable containing the results of the query.</returns>
    public async Task<DataTable> ExecuteDataTableAsync(string commandText, CommandType commandType = CommandType.Text,
        Dictionary<string, object?>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            throw new ArgumentException("Command text cannot be null or empty.", nameof(commandText));

        try
        {
            var connection = await GetConnectionAsync();
            using var command = new SqlCommand(commandText, connection)
            {
                CommandType = commandType,
                CommandTimeout = 30
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            await Task.Run(() => adapter.Fill(dataTable));
            return dataTable;
        }
        catch (SqlException ex)
        {
            throw new YoutubeShortsAutomatorException($"Database table query failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new YoutubeShortsAutomatorException("Unexpected error during ExecuteDataTableAsync.", ex);
        }
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
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
