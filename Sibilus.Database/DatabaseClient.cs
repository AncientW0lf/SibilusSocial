using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using System.Linq;

namespace Sibilus.Database
{
    /// <summary>
    /// A client to easily connect to and use a SQLite database.
    /// </summary>
    public class DatabaseClient : IDisposable
    {
        /// <summary>
        /// The underlying connection object used for all queries.
        /// </summary>
        private SqliteConnection _connection;

        /// <summary>
        /// Creates a new client and automatically connects it to the mentioned file. Creates the file if it doesn't exist.
        /// </summary>
        /// <param name="file">The complete or relative path of the file to open.</param>
        public DatabaseClient(string file)
        {
            // if (!File.Exists(file))
            //     File.Create(file).Dispose();

            string connection = new SqliteConnectionStringBuilder
            {
                DataSource = file,
            }.ToString();
            _connection = new SqliteConnection(connection);
            _connection.Open();
        }

        /// <summary>
        /// Creates a new database table with the specified columns.
        /// </summary>
        /// <param name="table">The name of the new table.</param>
        /// <param name="columns">All column infos for the new table.</param>
        public async Task CreateTableAsync(string table, params DbColumn[] columns)
        {
            SqliteCommand command;
            if (columns.Count(col => col.IsPrimary) > 1)
            {
                IEnumerable<string> primaries = columns.Where(a => a.IsPrimary).Select(b => b.Name);
                IEnumerable<string> definitions = columns.Select(c => c.ToString(true));

                command = new SqliteCommand(
                    $"create table {table}({string.Join(',', definitions)}, PRIMARY KEY({string.Join(',', primaries)}))",
                    _connection);
            }
            else
            {
                command = new SqliteCommand(
                    $"create table {table}({string.Join(',', columns)})",
                    _connection);
            }

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Checks if a table exists in the currently opened database.
        /// </summary>
        /// <param name="table">The name of the table to check for.</param>
        /// <returns>Returns whether the specified table exists in the database.</returns>
        public async Task<bool> TableExistsAsync(string table)
        {
            var command = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'", _connection);

            return await command.ExecuteScalarAsync() != null;
        }

        /// <summary>
        /// Writes new values into a database table.
        /// </summary>
        /// <param name="table">The table to write the new values into.</param>
        /// <param name="insertInfo">The new values to write. Must contain valid column names.</param>
        /// <returns>Returns the number of rows created.</returns>
        public async Task<int> WriteAsync(string table, params (string column, string value)[] insertInfo)
        {
            for (int i = 0; i < insertInfo.Length; i++)
                insertInfo[i].value = $"'{insertInfo[i].value}'";

            string columns = string.Join(',', insertInfo.Select(info => info.column));
            string values = string.Join(',', insertInfo.Select(info => info.value));

            var command = new SqliteCommand(
                $"insert into {table}({columns}) " +
                $"values({values})",
                _connection);

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Reads values from a database table.
        /// </summary>
        /// <param name="table">The table to read the values from.</param>
        /// <param name="columns">The columns to read values from.</param>
        /// <param name="maxRows">The maximum amount of rows returned.</param>
        /// <param name="conditions">A list of conditions to apply to this reading operation. Can be null.</param>
        /// <param name="unique">Boolean to enable or disable to only return unique values.</param>
        /// <returns>Returns an enumerable list of read values. The array is sorted like the columns that were specified.</returns>
        public async IAsyncEnumerable<object[]> ReadAsync(
            string table,
            IEnumerable<string> columns,
            int maxRows,
            IEnumerable<string> conditions = null,
            bool unique = false)
        {
            string colsString = string.Join(',', columns);
            string condsString = conditions != null
                ? $"where {string.Join(" and ", conditions)} "
                : "";

            var command = new SqliteCommand(
                $"select {(unique ? "distinct " : "")}{colsString} from {table} " +
                condsString +
                $"limit {maxRows}",
                _connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new object[reader.FieldCount];
                reader.GetValues(row);
                yield return row;
            }
        }

        /// <summary>
        /// Tests the connection by running a basic SQL version command through the database.
        /// </summary>
        /// <returns>
        /// Returns whether the command was successful.
        /// </returns>
        public async Task<bool> TestConnectionAsync()
        {
            var command = new SqliteCommand("select SQLITE_VERSION()", _connection);
            try
            {
                return await command.ExecuteScalarAsync() != null;
            }
            catch (System.Data.Common.DbException)
            {
                return false;
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}