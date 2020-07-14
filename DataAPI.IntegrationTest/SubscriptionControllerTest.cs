using System.Collections.Generic;
using System.Linq;
using System.Net;
using DataAPI.Client;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.UserManagement;
using DataAPI.IntegrationTest.DataObjects;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class SubscriptionControllerTest : ApiTestBase
    {
        [Test]
        public void CanSubscribeAndUnsubscribe()
        {
            string subscriptionId = null;
            AssertStatusCode(
                () => subscriptionId = SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created),
                HttpStatusCode.OK, "Subscribe");
            AssertStatusCode(
                () => analystDataApiClient.UnsubscribeAsync(subscriptionId).Wait(),
                HttpStatusCode.OK, "Unsubscribe");
        }

        [Test]
        public void CanSubscribeMultipleTimesToSameDataType()
        {
            // Can subscribe with different parameters
            AssertStatusCode(
                () => SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created),
                HttpStatusCode.OK, "Subscribe (first time)");
            AssertStatusCode(
                () => SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Replaced),
                HttpStatusCode.OK, "Subscribe (second time)");
            AssertStatusCode(
                () => UnsubscribeFromUnitTestSearchObject(analystDataApiClient),
                HttpStatusCode.OK, "Unsubscribe");
        }

        [Test]
        public void SubscribingTwiceResultsInConflict()
        {
            // Cannot subscribe with the same parameters twice.
            // This is to prevent duplicate subscriptions
            AssertStatusCode(
                () => SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created),
                HttpStatusCode.OK, "Subscribe (first time)");
            AssertStatusCode(
                () => SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created),
                HttpStatusCode.Conflict, "Subscribe (second time)");
            AssertStatusCode(
                () => UnsubscribeFromUnitTestSearchObject(analystDataApiClient),
                HttpStatusCode.OK, "Unsubscribe");
        }

        [Test]
        public void GetSubscriptionsReturnsExpectedSubscriptions()
        {
            var subscriptions = analystDataApiClient.GetSubscriptionsAsync().Result;
            Assume.That(subscriptions, Is.Empty);
            try
            {
                SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created);
                subscriptions = analystDataApiClient.GetSubscriptionsAsync().Result;
                Assert.That(subscriptions.Count, Is.EqualTo(1));
            }
            finally
            {
                UnsubscribeFromUnitTestSearchObject(analystDataApiClient);
            }
        }

        [Test]
        public void NotificationsAreCreatedForSubscribedData()
        {
            // Setup
            AssertStatusCode(
                () => SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created),
                HttpStatusCode.OK, "Subscribe");
            var searchObjectCount = 2;
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(searchObjectCount, analystDataApiClient);

            // Test
            try
            {
                List<SubscriptionNotification> notifications = null;
                AssertStatusCode(
                    () => notifications = analystDataApiClient.GetSubscribedObjects().Result,
                    HttpStatusCode.OK, "Get notifications (first time)");
                Assert.That(notifications.Count, Is.EqualTo(searchObjectCount), "Notification count (first time)");

                // Test that notifications 
                AssertStatusCode(
                    () => notifications = analystDataApiClient.GetSubscribedObjects().Result,
                    HttpStatusCode.OK, "Get notifications (second time)");
                Assert.That(notifications.Count, Is.EqualTo(searchObjectCount), "Notification count (second time)");

                foreach (var notification in notifications)
                {
                    AssertStatusCode(
                        () => analystDataApiClient.DeleteNotificationAsync(notification.Id).Wait(),
                        HttpStatusCode.OK, "Delete notification");
                }
                // Test that notifications have been deleted
                AssertStatusCode(
                    () => notifications = analystDataApiClient.GetSubscribedObjects().Result,
                    HttpStatusCode.OK, "Get notifications after deletion");
                Assert.That(notifications.Count, Is.EqualTo(0), "Notification count after deletion");
            }
            finally
            {
                // Tear down
                SearchDataGenerator.DeleteData(submittedData, analystDataApiClient);
                UnsubscribeFromUnitTestSearchObject(analystDataApiClient);
            }
        }

        [Test]
        public void DataModificationTypeAsExpected()
        {
            var testObject = SearchDataGenerator.GenerateUnitTestSearchObject();
            string subscriptionId = null;
            AssertStatusCode(
                () => subscriptionId = SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created, DataModificationType.Replaced),
                HttpStatusCode.OK, "Subscribe");

            try
            {
                // Create object
                analystDataApiClient.InsertAsync(testObject, testObject.Id).Wait();
                var notifications = analystDataApiClient.GetSubscribedObjects().Result;
                Assert.That(notifications.Count, Is.EqualTo(1));
                Assert.That(notifications.Single().ModificationType, Is.EqualTo(DataModificationType.Created));
                analystDataApiClient.DeleteNotificationAsync(notifications.Single().Id).Wait();

                // Replace object
                analystDataApiClient.ReplaceAsync(testObject, testObject.Id).Wait();
                notifications = analystDataApiClient.GetSubscribedObjects().Result;
                Assert.That(notifications.Count, Is.EqualTo(1));
                Assert.That(notifications.Single().ModificationType, Is.EqualTo(DataModificationType.Replaced));
                analystDataApiClient.DeleteNotificationAsync(notifications.Single().Id).Wait();

                // Delete object
                analystDataApiClient.DeleteAsync<UnitTestSearchObject>(testObject.Id).Wait();
                notifications = analystDataApiClient.GetSubscribedObjects().Result;
                Assert.That(notifications.Count, Is.EqualTo(1));
                Assert.That(notifications.Single().ModificationType, Is.EqualTo(DataModificationType.Deleted));
                analystDataApiClient.DeleteNotificationAsync(notifications.Single().Id).Wait();
            }
            finally
            {
                UnsubscribeFromUnitTestSearchObject(analystDataApiClient);
            }
        }

        [Test]
        public void EmptyModificationTypesListIsRejected()
        {
            try
            {
                AssertStatusCode(
                    () => SubscribeToUnitTestSearchObject(analystDataApiClient),
                    HttpStatusCode.BadRequest);
            }
            finally
            {
                UnsubscribeFromUnitTestSearchObject(analystDataApiClient);
            }
        }

        [Test]
        public void UnknownValueInModificationTypesListIsRejected()
        {
            try
            {
                AssertStatusCode(
                    () => SubscribeToUnitTestSearchObject(analystDataApiClient, DataModificationType.Created, DataModificationType.Unknown),
                    HttpStatusCode.BadRequest);
            }
            finally
            {
                UnsubscribeFromUnitTestSearchObject(analystDataApiClient);
            }
        }

        [Test]
        public void UsersCanReportDataToEachOther()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Viewer, adminDataApiClient, out var otherDataApiClient);

            var dataType = nameof(UnitTestDataObject2);
            var dataObjectId = "FakeID";
            try
            {
                AssertStatusCode(
                    () => analystDataApiClient.ReportTo(otherDataApiClient.LoggedInUsername, dataType, dataObjectId).Wait(),
                    HttpStatusCode.OK);
                List<SubscriptionNotification> notifications = null;
                AssertStatusCode(
                    () => notifications = otherDataApiClient.GetSubscribedObjects().Result,
                    HttpStatusCode.OK);
                Assert.That(notifications.Count, Is.EqualTo(1));
                var notification = notifications.Single();
                Assert.That(notification.DataType, Is.EqualTo(dataType));
                Assert.That(notification.DataObjectId, Is.EqualTo(dataObjectId));
                Assert.That(notification.ModificationType, Is.EqualTo(DataModificationType.Created));
                AssertStatusCode(
                    () => otherDataApiClient.DeleteNotificationAsync(notification.Id).Wait(),
                    HttpStatusCode.OK);
            }
            finally
            {
                UserGenerator.DeleteUser(otherDataApiClient);
            }
        }

        private string SubscribeToUnitTestSearchObject(IDataApiClient dataApiClient, params DataModificationType[] modificationTypes)
        {
            return dataApiClient.SubscribeAsync(
                    nameof(UnitTestSearchObject),
                    modificationTypes)
                .Result;
        }

        private void UnsubscribeFromUnitTestSearchObject(IDataApiClient dataApiClient)
        {
            dataApiClient.UnsubscribeAllAsync(
                    nameof(UnitTestSearchObject))
                .Wait();
        }
    }
}
