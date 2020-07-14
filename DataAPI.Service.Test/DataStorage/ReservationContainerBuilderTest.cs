using System;
using System.Data.SqlClient;
using Commons.Misc;
using DataAPI.Service.DataStorage;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    [Category("IntegrationTest")]
    public class ReservationContainerBuilderTest
    {
        [Test]
        public void CanBuildReservationContainer()
        {
            var sut = CreateReservationContainerBuilder();
            var submitter = "jdoe";
            GenericDataContainer actual = null;
            Assert.That(() => actual = sut.Build(submitter), Throws.Nothing);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.OriginalSubmitter, Is.EqualTo(submitter));
            Assert.That(actual.CreatedTimeUtc, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(3)));
            Assert.That(actual.Submitter, Is.EqualTo(submitter));
            Assert.That(actual.SubmissionTimeUtc, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(3)));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual.Data.TryGetElement("source_system", out _), Is.True, "source_system exists");
            Assert.That(actual.Data.TryGetElement("source_table", out _), Is.True, "source_table exists");
            Assert.That(actual.Data.TryGetElement("component_type", out _), Is.True, "component_type exists");
            Assert.That(actual.Data.TryGetElement("is_deleted", out _), Is.True, "is_deleted exists");
        }

        private ReservationContainerBuilder CreateReservationContainerBuilder()
        {
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "myserver",
                UserID = "jdoe_dataapi",
                Password = Secrets.Get("DataAPI_SqlPassword_myserver")
            }.ConnectionString;
            var tableName = "[DataApiSqlIntegrationUnitTest].[dbo].[Component]";
            var queryExecutor = new SqlQueryExecutor(sqlConnectionString);
            return new ReservationContainerBuilder(queryExecutor, tableName);
        }
    }
}
