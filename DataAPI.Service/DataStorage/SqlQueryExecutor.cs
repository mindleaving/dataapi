using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DataAPI.Service.DataStorage
{
    public class SqlQueryExecutor
    {
        private readonly string sqlConnectionString;

        public SqlQueryExecutor(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        // CAUTION: Do proper testing if changing to 'await using'.
        // At time of writing 'await using' results in EntryPointNotFoundException.
        public async Task<int> ExecuteQueryAsync(string query)
        {
            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                await sqlConnection.OpenAsync();
                try
                {
                    using (var sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        return await sqlCommand.ExecuteNonQueryAsync();
                    }
                }
                catch (SqlException sqlException)
                {
                    throw new FormatException($"{sqlException.Message} (Line {sqlException.LineNumber}, character {sqlException.Number})");
                }
            }
        }

        // CAUTION: Do proper testing if changing to 'await using'.
        // At time of writing 'await using' results in EntryPointNotFoundException.
        public async IAsyncEnumerable<IDataRecord> ExecuteReaderAsync(string query)
        {
            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                await sqlConnection.OpenAsync();
                using (var sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    SqlDataReader reader;
                    try
                    {
                        reader = await sqlCommand.ExecuteReaderAsync();
                    }
                    catch (SqlException sqlException)
                    {
                        throw new FormatException($"{sqlException.Message} (Line {sqlException.LineNumber}, character {sqlException.Number})");
                    }
                    while (await reader.ReadAsync())
                    {
                        yield return reader;
                    }
                    reader.Dispose();
                }
            }
        }
    }
}
