// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension helpers for DatabaseContext
// =============================================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using YouTubeShortAutomator.Data;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Provides extension methods for <see cref="DatabaseContext"/> that simplify
/// common query patterns.
/// </summary>
public static class DatabaseContextExtensions
{
    /// <summary>
    /// Executes a query and maps each result row to a strongly‑typed object.
    /// </summary>
    /// <typeparam name="T">The type of the objects to return.</typeparam>
    /// <param name="db">The <see cref="DatabaseContext"/> instance.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="mapper">A function that maps an <see cref="IDataRecord"/> to <typeparamref name="T"/>.</param>
    /// <param name="parameters">
    /// Optional query parameters. When <c>null</c> no parameters are added.
    /// </param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> containing the mapped results.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="db"/> or <paramref name="mapper"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sql"/> is <c>null</c> or whitespace.
    /// </exception>
    /// <exception cref="YoutubeShortsAutomatorException">
    /// Propagated from the underlying <see cref="DatabaseContext"/> methods.
    /// </exception>
    public static async Task<IReadOnlyList<T>> QueryAsync<T>(
        this DatabaseContext db,
        string sql,
        Func<IDataRecord, T> mapper,
        Dictionary<string, object?>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(mapper);

        // Retrieve the raw DataTable from the context.
        var dataTable = await db.ExecuteDataTableAsync(sql, CommandType.Text, parameters);

        var results = new List<T>(dataTable.Rows.Count);
        using var reader = dataTable.CreateDataReader();

        while (reader.Read())
        {
            results.Add(mapper(reader));
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Executes a query and returns the first mapped result, or <c>default</c> if no rows are returned.
    /// </summary>
    /// <typeparam name="T">The type of the object to return.</typeparam>
    /// <param name="db">The <see cref="DatabaseContext"/> instance.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="mapper">A function that maps an <see cref="IDataRecord"/> to <typeparamref name="T"/>.</param>
    /// <param name="parameters">
    /// Optional query parameters. When <c>null</c> no parameters are added.
    /// </param>
    /// <returns>The first mapped result, or <c>default</c> if the result set is empty.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="db"/> or <paramref name="mapper"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sql"/> is <c>null</c> or whitespace.
    /// </exception>
    /// <exception cref="YoutubeShortsAutomatorException">
    /// Propagated from the underlying <see cref="DatabaseContext"/> methods.
    /// </exception>
    public static async Task<T?> QuerySingleAsync<T>(
        this DatabaseContext db,
        string sql,
        Func<IDataRecord, T> mapper,
        Dictionary<string, object?>? parameters = null)
    {
        var list = await db.QueryAsync(sql, mapper, parameters);
        return list.FirstOrDefault();
    }

    /// <summary>
    /// Executes a batch of non‑query commands (e.g., INSERT, UPDATE, DELETE) and returns the total number of rows affected.
    /// </summary>
    /// <param name="db">The <see cref="DatabaseContext"/> instance.</param>
    /// <param name="commands">A collection of SQL command texts to execute.</param>
    /// <param name="commandType">
    /// The <see cref="CommandType"/> of the supplied commands. Defaults to <see cref="CommandType.Text"/>.
    /// </param>
    /// <returns>The sum of rows affected by all commands.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="db"/> or <paramref name="commands"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="YoutubeShortsAutomatorException">
    /// Propagated from the underlying <see cref="DatabaseContext"/> methods.
    /// </exception>
    public static async Task<int> ExecuteBatchAsync(
        this DatabaseContext db,
        IEnumerable<string> commands,
        CommandType commandType = CommandType.Text)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(commands);

        var total = 0;
        foreach (var cmd in commands)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                continue;

            total += await db.ExecuteCommandAsync(cmd, commandType);
        }

        return total;
    }
}
