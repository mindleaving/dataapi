using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlMigrationTools
{
    public interface ISqlMigration
    {
        Task RunCommandAsync(string command);
        Task BuildResultAsync(
            string query, 
            Action<SqlDataReader> resultBuildingFunc,
            CancellationToken cancellationToken = default);
        Task BuildResultFromStoredProcedureAsync(
            string procedureName, 
            Dictionary<string, object> parameters, 
            Action<SqlDataReader> resultBuildingFunc,
            CancellationToken cancellationToken = default);

        Task DeleteMatchingAsync(
            string tableName,
            string whereClause);

        Task BatchUploadAsync<T>(
            IReadOnlyList<T> inputs,
            string tableName,
            IList<string> columnNames,
            Func<T, IEnumerable<IList<string>>> transformFunc);

        Task<bool> ExistsAsync(string tableName, string whereClause);
    }
}