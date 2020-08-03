using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.UserManagement;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Workflow
{
    public class UserProfileLoader
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IReadonlyObjectDatabase<UserProfile> userDatabase;

        public UserProfileLoader(
            IDataApiClient dataApiClient,
            IReadonlyObjectDatabase<UserProfile> userDatabase)
        {
            this.dataApiClient = dataApiClient;
            this.userDatabase = userDatabase;
        }

        public async Task<List<UserProfileViewModel>> LoadAsync()
        {
            var userProfileViewModels = new List<UserProfileViewModel>();
            var userProfiles = await userDatabase.GetManyAsync();
            foreach (var userProfile in userProfiles)
            {
                userProfileViewModels.Add(await CreateUserProfileViewModel(userProfile));
            }
            return userProfileViewModels;
        }

        private async Task<UserProfileViewModel> CreateUserProfileViewModel(UserProfile x)
        {
            var roles = await dataApiClient.GetGlobalRolesForUser(x.Username);
            return new UserProfileViewModel(x, roles);
        }
    }
}
