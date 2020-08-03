using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SqlMigrationTools.Test
{
    [TestFixture]
    public class SqlMigrationTest
    {
        private const string DatabaseName = "SqlMigrationToolsTest";
        private const string TableName = "TestData";
        private readonly string connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "(localdb)\\MSSQLLocalDB",
            IntegratedSecurity = true,
            InitialCatalog = DatabaseName
        }.ConnectionString;
        private SqlMigration sut;
        

        [OneTimeSetUp]
        public async Task CreateTestTable()
        {
            sut = new SqlMigration(connectionString);
            var connectionStringWithoutDatabase = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = string.Empty
            }.ConnectionString;
            await using var sqlConnection = new SqlConnection(connectionStringWithoutDatabase);
            await sqlConnection.OpenAsync();
            try
            {
                await sqlConnection.ChangeDatabaseAsync(DatabaseName);
            }
            catch
            {
                await using var createDatabaseCommand = new SqlCommand($"CREATE DATABASE {DatabaseName}", sqlConnection);
                await createDatabaseCommand.ExecuteNonQueryAsync();
                await sqlConnection.ChangeDatabaseAsync(DatabaseName);
            }

            if (!await sut.TableExistsAsync(TableName))
            {
                var createTableCommandText = $"CREATE TABLE [{TableName}] ("
                                             + $"[{nameof(TestDataEntry.Id)}] INT NOT NULL PRIMARY KEY, "
                                             + $"[{nameof(TestDataEntry.Comment)}] VARCHAR(MAX) NULL, "
                                             + $"[{nameof(TestDataEntry.DoubleValue)}] DECIMAL(18, 2) NOT NULL,"
                                             + "NotUsed INT NULL)";
                await using var createTableCommand = new SqlCommand(createTableCommandText, sqlConnection);
                await createTableCommand.ExecuteNonQueryAsync();
            }
        }

        [SetUp]
        public async Task ResetTable()
        {
            await using var sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            await sqlConnection.ChangeDatabaseAsync(DatabaseName);
            await using var clearTableCommand = new SqlCommand($"DELETE FROM [{TableName}]", sqlConnection);
            await clearTableCommand.ExecuteNonQueryAsync();
        }

        [Test]
        [TestCase(613, "Hello, 'world'!", 13.37)]
        [TestCase(613, null, -1.2e-1)]
        public async Task CanInsertAndGetData(int id, string comment, double doubleValue)
        {
            var expected = new TestDataEntry(id, comment, doubleValue);
            await sut.BatchUploadAsync(
                new[] {expected},
                TableName,
                new[] {nameof(TestDataEntry.Id), nameof(TestDataEntry.Comment), nameof(TestDataEntry.DoubleValue)},
                x => new[] {new[] {x.Id.ToString(), x.Comment, x.DoubleValue.ToString("F2", CultureInfo.InvariantCulture)}});
            var readData = new List<TestDataEntry>();
            await sut.BuildResultAsync($"SELECT * FROM {TableName}", reader =>
            {
                var id = SqlHelpers.ReadInt(reader, nameof(TestDataEntry.Id));
                var comment = SqlHelpers.ReadString(reader, nameof(TestDataEntry.Comment));
                var doubleValue = SqlHelpers.ReadDouble(reader, nameof(TestDataEntry.DoubleValue));

                readData.Add(new TestDataEntry(id.Value, comment, doubleValue.Value));
            });
            Assert.That(readData.Count, Is.EqualTo(1));
            var actual = readData.Single();
            Assert.That(actual.Id, Is.EqualTo(expected.Id));
            Assert.That(actual.Comment, Is.EqualTo(expected.Comment));
            Assert.That(actual.DoubleValue, Is.EqualTo(expected.DoubleValue).Within(1e-6));
        }

        private class TestDataEntry
        {
            public TestDataEntry(
                int id,
                string comment,
                double doubleValue)
            {
                Id = id;
                Comment = comment;
                DoubleValue = doubleValue;
            }

            public int Id { get; }
            public string Comment { get; }
            public double DoubleValue { get; }
        }
    }
}