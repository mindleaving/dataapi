using System.Collections.Generic;
using System.Linq;
using System.Net;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.UserManagement;
using DataAPI.IntegrationTest.DataObjects;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class AccountControllerTest : ApiTestBase
    {
        [Test]
        public void CanRegisterLoginAndDeleteUser()
        {
            Assume.That(adminDataApiClient.IsAvailable(), "API not available");
            var dataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);

            var username = UserGenerator.GenerateUsername();
            var password = UserGenerator.GeneratePassword();
            var email = $"{username}@example.org";
            var firstName = "Jamie";
            var lastName = "Doe";
            Assert.That(() => dataApiClient.Register(username, firstName, lastName, password, email), Throws.Nothing);
            AuthenticationResult authenticationResult = null;
            Assert.That(() => authenticationResult = dataApiClient.Login(username, password), Throws.Nothing);
            Assert.That(authenticationResult.IsAuthenticated, Is.True);
            Assert.That(() => dataApiClient.DeleteUser(username), Throws.Nothing);
            dataApiClient.Logout();
            Assert.That(() => authenticationResult = dataApiClient.Login(username, password), Throws.Nothing);
            Assert.That(authenticationResult.IsAuthenticated, Is.False);
        }

        [Test]
        public void UserCanChangeOwnPassword()
        {
            UserGenerator.RegisterAndLoginUserWithoutRoles(out var dataApiClient);

            try
            {
                var newPassword = UserGenerator.GeneratePassword();
                AssertStatusCode(
                    () => dataApiClient.ChangePassword(dataApiClient.LoggedInUsername, newPassword),
                    HttpStatusCode.OK, "Change password");
                AuthenticationResult authenticationResult = null;
                AssertStatusCode(
                    () => authenticationResult = dataApiClient.Login(dataApiClient.LoggedInUsername, newPassword),
                    HttpStatusCode.OK, "Login with new password");
                Assert.That(authenticationResult.IsAuthenticated, Is.True);
            }
            finally
            {
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void NonManagerUserCannotChangePasswordOfOtherUser()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var analyst2DataApiClient);

            try
            {
                var newPassword = UserGenerator.GeneratePassword();
                AssertStatusCode(
                    () => analystDataApiClient.ChangePassword(analyst2DataApiClient.LoggedInUsername, newPassword),
                    HttpStatusCode.Unauthorized);
            }
            finally
            {
                UserGenerator.DeleteUser(analyst2DataApiClient);
            }
        }

        [Test]
        public void NonManagerUserCannotAddRoles()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var analyst2DataApiClient);

            try
            {
                // User should not be able to add role to themself
                AssertStatusCode(
                    () => analyst2DataApiClient.AddGlobalRoleToUser(analyst2DataApiClient.LoggedInUsername, Role.Admin),
                    HttpStatusCode.Unauthorized, "Add role to self");
                // User should not be able to add role to other user
                AssertStatusCode(
                    () => analystDataApiClient.AddGlobalRoleToUser(analyst2DataApiClient.LoggedInUsername, Role.Admin),
                    HttpStatusCode.Unauthorized, "Add role to other user");
            }
            finally
            {
                UserGenerator.DeleteUser(analyst2DataApiClient);
            }
        }

        [Test]
        public void NonManagerUserCannotDeleteOtherUsers()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var analyst2DataApiClient);

            try
            {
                // User should not be able to add role to themself
                AssertStatusCode(
                    () => analystDataApiClient.DeleteUser(analyst2DataApiClient.LoggedInUsername),
                    HttpStatusCode.Unauthorized);
            }
            finally
            {
                UserGenerator.DeleteUser(analyst2DataApiClient);
            }
        }

        [Test]
        public void ManagerCanAddRoles()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.UserManager, adminDataApiClient, out var managerDataApiClient);
            UserGenerator.RegisterAndLoginUserWithoutRoles(out var unpriviligedDataApiClient);

            try
            {
                AssertStatusCode(
                    () => managerDataApiClient.AddGlobalRoleToUser(unpriviligedDataApiClient.LoggedInUsername, Role.Admin),
                    HttpStatusCode.OK);
            }
            finally
            {
                UserGenerator.DeleteUser(managerDataApiClient);
                UserGenerator.DeleteUser(unpriviligedDataApiClient);
            }
        }

        [Test]
        public void ManagerCannotAddRolesToThemself()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.UserManager, adminDataApiClient, out var managerDataApiClient);

            try
            {
                AssertStatusCode(
                    () => managerDataApiClient.AddGlobalRoleToUser(managerDataApiClient.LoggedInUsername, Role.Admin),
                    HttpStatusCode.Unauthorized);
            }
            finally
            {
                UserGenerator.DeleteUser(managerDataApiClient);
            }
        }

        [Test]
        public void ManagerCanSeeRoles()
        {
            var userRole = Role.DataProducer;
            UserGenerator.RegisterAndLoginUserWithRole(Role.UserManager, adminDataApiClient, out var managerDataApiClient);
            UserGenerator.RegisterAndLoginUserWithRole(userRole, adminDataApiClient, out var dataApiClient);

            try
            {
                List<Role> roles = null;
                AssertStatusCode(
                    () => roles = managerDataApiClient.GetGlobalRolesForUser(dataApiClient.LoggedInUsername).Result,
                    HttpStatusCode.OK);
                Assert.That(roles, Is.Not.Null);
                Assert.That(roles, Is.EquivalentTo(new[] { userRole }));
            }
            finally
            {
                UserGenerator.DeleteUser(managerDataApiClient);
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void NonManagerCannotSeeRoles()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var analyst2DataApiClient);

            try
            {
                AssertStatusCode(
                    () => analystDataApiClient.GetGlobalRolesForUser(analyst2DataApiClient.LoggedInUsername).Wait(),
                    HttpStatusCode.Unauthorized);
            }
            finally
            {
                UserGenerator.DeleteUser(analyst2DataApiClient);
            }
        }

        [Test]
        public void UserCanSeeOwnRoles()
        {
            var userRole = Role.Analyst;
            UserGenerator.RegisterAndLoginUserWithRole(userRole, adminDataApiClient, out var dataApiClient);

            try
            {
                List<Role> roles = null;
                AssertStatusCode(
                    () => roles = dataApiClient.GetGlobalRolesForUser(dataApiClient.LoggedInUsername).Result,
                    HttpStatusCode.OK);
                Assert.That(roles, Is.Not.Null);
                Assert.That(roles, Is.EquivalentTo(new[] { userRole }));
            }
            finally
            {
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void ManagerCanDeleteUser()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.UserManager, adminDataApiClient, out var managerDataApiClient);
            UserGenerator.RegisterAndLoginUserWithoutRoles(out var unprivilegedDataApiClient);

            try
            {
                AssertStatusCode(
                    () => managerDataApiClient.DeleteUser(unprivilegedDataApiClient.LoggedInUsername),
                    HttpStatusCode.OK);
            }
            catch (AssertionException)
            {
                UserGenerator.DeleteUser(unprivilegedDataApiClient);
            }
            finally
            {
                UserGenerator.DeleteUser(managerDataApiClient);
            }
        }

        [Test]
        public void ManagerCanSetRoles()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.UserManager, adminDataApiClient, out var managerDataApiClient);
            UserGenerator.RegisterAndLoginUserWithoutRoles(out var unprivilegedDataApiClient);

            var collectionName = nameof(UnitTestDataObject1);
            MakeCollectionProtected(collectionName);
            try
            {
                AssertStatusCode(
                    () => managerDataApiClient.SetCollectionRoleForUser(unprivilegedDataApiClient.LoggedInUsername, new [] {Role.Viewer}, collectionName),
                    HttpStatusCode.OK);
                CollectionInformation collectionPermissions = null;
                AssertStatusCode(
                    () => collectionPermissions = unprivilegedDataApiClient.GetCollectionInformationAsync(collectionName).Result,
                    HttpStatusCode.OK);
                Assert.That(collectionPermissions, Is.Not.Null);
                Assert.That(collectionPermissions.UserRoles.Count, Is.EqualTo(1));
                Assert.That(collectionPermissions.UserRoles.Single(), Is.EqualTo(Role.Viewer));
            }
            finally
            {
                MakeCollectionUnprotected(collectionName);
                UserGenerator.DeleteUser(unprivilegedDataApiClient);
                UserGenerator.DeleteUser(managerDataApiClient);
            }
        }

        [Test]
        public void RolesCanBeSetMoreThanOnce()
        {
            // Sounds silly, but relates to bug-2835
            UserGenerator.RegisterAndLoginUserWithRole(Role.UserManager, adminDataApiClient, out var managerDataApiClient);
            UserGenerator.RegisterAndLoginUserWithoutRoles(out var unprivilegedDataApiClient);

            var collectionName = nameof(UnitTestDataObject1);
            MakeCollectionProtected(collectionName);
            try
            {
                AssertStatusCode(
                    () => managerDataApiClient.SetCollectionRoleForUser(
                        unprivilegedDataApiClient.LoggedInUsername, 
                        new [] {Role.Viewer}, 
                        collectionName),
                    HttpStatusCode.OK);
                var collectionPermissions = unprivilegedDataApiClient.GetCollectionInformationAsync(collectionName).Result;
                Assert.That(collectionPermissions.UserRoles.Count, Is.EqualTo(1));
                Assert.That(collectionPermissions.UserRoles.Single(), Is.EqualTo(Role.Viewer));

                AssertStatusCode(
                    () => managerDataApiClient.SetCollectionRoleForUser(
                        unprivilegedDataApiClient.LoggedInUsername,
                        new[] { Role.Viewer, Role.UserManager },
                        collectionName),
                    HttpStatusCode.OK);
                collectionPermissions = unprivilegedDataApiClient.GetCollectionInformationAsync(collectionName).Result;
                Assert.That(collectionPermissions.UserRoles, Is.EquivalentTo(new[] { Role.Viewer, Role.UserManager}));
            }
            finally
            {
                MakeCollectionUnprotected(collectionName);
                UserGenerator.DeleteUser(unprivilegedDataApiClient);
                UserGenerator.DeleteUser(managerDataApiClient);
            }
        }

        [Test]
        public void ViewerCanGetAllUserProfiles()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Viewer, adminDataApiClient, out var viewerDataApiClient);

            try
            {
                List<UserProfile> userProfiles = null;
                AssertStatusCode(
                    () => userProfiles = viewerDataApiClient.GetAllUserProfiles().Result,
                    HttpStatusCode.OK);
                Assert.That(userProfiles, Is.Not.Null);
                Assert.That(userProfiles, Has.One.Matches<UserProfile>(profile => profile.Username == viewerDataApiClient.LoggedInUsername));
            }
            finally
            {
                UserGenerator.DeleteUser(viewerDataApiClient);
            }
        }

        [Test]
        public void CanViewCollectionPermissionsForProtectedCollection()
        {
            UserGenerator.RegisterAndLoginUserWithoutRoles(out var unpriviligedDataApiClient);

            var collectionName = nameof(UnitTestDataObject1);
            MakeCollectionProtected(collectionName);
            adminDataApiClient.AddCollectionRoleToUser(unpriviligedDataApiClient.LoggedInUsername, Role.Analyst, collectionName);
            try
            {
                List<CollectionUserPermissions> collectionPermissions = null;
                AssertStatusCode(
                    () => collectionPermissions = adminDataApiClient.GetCollectionPermissions(collectionName).Result,
                    HttpStatusCode.OK);
                Assert.That(collectionPermissions, Is.Not.Null);
                Assert.That(collectionPermissions.Count(x => x.Username == unpriviligedDataApiClient.LoggedInUsername), Is.EqualTo(1));
                Assert.That(collectionPermissions.Single(x => x.Username == unpriviligedDataApiClient.LoggedInUsername).Roles, Contains.Item(Role.Analyst));
            }
            finally
            {
                MakeCollectionUnprotected(collectionName);
                UserGenerator.DeleteUser(unpriviligedDataApiClient);
            }
        }

        [Test]
        public void CollectionPermissionsEmptyForUnprotectedCollection()
        {
            var collectionName = nameof(UnitTestDataObject1);
            MakeCollectionUnprotected(collectionName);

            List<CollectionUserPermissions> collectionPermissions = null;
            AssertStatusCode(
                () => collectionPermissions = adminDataApiClient.GetCollectionPermissions(collectionName).Result,
                HttpStatusCode.OK);
            Assert.That(collectionPermissions, Is.Not.Null);
            Assert.That(collectionPermissions, Is.Empty);
        }
    }
}
