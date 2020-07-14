using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAPI.Client.Communicators;
using DataAPI.Client.UtilityFunctions;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.UserManagement;
using DataAPI.DataStructures.Validation;
using DataAPI.DataStructures.Views;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client
{
    /// <inheritdoc />
    public class DataApiClient : IDataApiClient
    {
        private readonly IHttpClientProxy httpClientProxy;
        private readonly LoginManager loginManager;
        private readonly ExpressionParser expressionParser;

        public DataApiClient(ApiConfiguration apiConfiguration, bool ignoreSslNameMismatch = false)
            : this(apiConfiguration, new DataApiHttpClientProxy(true, ignoreSslNameMismatch))
        {
        }

        public DataApiClient(ApiConfiguration apiConfiguration, IHttpClientProxy httpClientProxy)
        {
            this.httpClientProxy = httpClientProxy;
            ApiConfiguration = apiConfiguration;
            loginManager = new LoginManager(apiConfiguration, this.httpClientProxy);
            expressionParser = new ExpressionParser();
        }

        public ApiConfiguration ApiConfiguration { get; }
        public LoginMethod LoginMethod
        {
            get => loginManager.LoginMethod;
            set => loginManager.LoginMethod = value;
        }
        public bool IsLoggedIn => loginManager.IsLoggedIn;
        public string LoggedInUsername => IsLoggedIn ? loginManager.LoggedInUsername : null;

        #region Service status
        public bool IsAvailable()
            => ServiceStatusCommunicator.IsAvailable(ApiConfiguration, httpClientProxy.Client);
        #endregion


        #region Account
        public Task<List<UserProfile>> GetAllUserProfiles()
            => AccountCommunicator.GetAllUserProfiles(ApiConfiguration, httpClientProxy.Client);

        public Task<List<Role>> GetGlobalRolesForUser(string username)
            => AccountCommunicator.GetGlobalRolesForUserAsync(ApiConfiguration, httpClientProxy.Client, username);

        public AuthenticationResult Login()
        {
            Logout();
            LoginMethod = LoginMethod.ActiveDirectory;
            return loginManager.Login();
        }

        public AuthenticationResult Login(string username, string password)
        {
            loginManager.Logout();
            loginManager.LoginMethod = LoginMethod.JsonWebToken;
            loginManager.LoginInformation = new LoginInformation(username, password);
            return loginManager.Login();
        }

        public AuthenticationResult RetryLogin()
        {
            return loginManager.Login(force: true);
        }

        public void SetAccessToken(string accessToken)
        {
            loginManager.SetAccessToken(accessToken);
        }

        public void Logout()
        {
            if(!IsLoggedIn)
                return;
            loginManager.Logout();
        }

        public void Register(string username, string firstName, string lastName, string password, string email)
            => AccountCommunicator.Register(ApiConfiguration, httpClientProxy.Client, username, firstName, lastName, password, email);

        public void ChangePassword(string username, string password)
            => AccountCommunicator.ChangePassword(ApiConfiguration, httpClientProxy.Client, username, password);

        public void AddGlobalRoleToUser(string username, Role role)
            => AccountCommunicator.AddGlobalRoleToUser(ApiConfiguration, httpClientProxy.Client, username, role);

        public void AddCollectionRoleToUser(string username, Role role, string dataType)
            => AccountCommunicator.AddCollectionRoleToUser(ApiConfiguration, httpClientProxy.Client, username, role, dataType);

        public void SetGlobalRolesForUser(string username, IList<Role> roles)
            => AccountCommunicator.SetGlobalRolesForUser(ApiConfiguration, httpClientProxy.Client, username, roles);

        /// <inheritdoc />
        public void SetCollectionRoleForUser(string username, IList<Role> roles, string dataType)
            => AccountCommunicator.SetCollectionRoleForUser(ApiConfiguration, httpClientProxy.Client, username, roles, dataType);

        /// <inheritdoc />
        public void RemoveGlobalRoleFromUser(string username, Role role)
            => AccountCommunicator.RemoveGlobalRoleFromUser(ApiConfiguration, httpClientProxy.Client, username, role);

        /// <inheritdoc />
        public void RemoveCollectionRoleFromUser(string username, Role role, string dataType)
            => AccountCommunicator.RemoveCollectionRoleFromUser(ApiConfiguration, httpClientProxy.Client, username, role, dataType);

        public void DeleteUser(string username)
            => AccountCommunicator.DeleteUser(ApiConfiguration, httpClientProxy.Client, username);

        public Task<List<CollectionUserPermissions>> GetCollectionPermissions(string dataType)
            => AccountCommunicator.GetCollectionPermissions(ApiConfiguration, httpClientProxy.Client, dataType);
        #endregion


        #region DataIO
        public Task<string> InsertAsync(object obj, string id = null)
            => DataIoCommunicator.InsertAsync(ApiConfiguration, httpClientProxy.Client, obj, id);
        
        public Task<string> InsertAsync(string dataType, string json, string id = null)
            => DataIoCommunicator.InsertAsync(ApiConfiguration, httpClientProxy.Client, dataType, json, id);
        
        public Task<string> ReplaceAsync(object obj, string existingId)
            => DataIoCommunicator.ReplaceAsync(ApiConfiguration, httpClientProxy.Client, obj, existingId);

        public Task<string> ReplaceAsync(string dataType, string json, string existingId)
            => DataIoCommunicator.ReplaceAsync(ApiConfiguration, httpClientProxy.Client, dataType, json, existingId);

        public Task<string> CreateSubmission<T>(T obj, Func<T, byte[]> binaryDataPath, string id = null)
            => DataIoCommunicator.CreateSubmission<T>(ApiConfiguration, httpClientProxy.Client, obj, binaryDataPath, id);

        public Task<string> CreateSubmission(JObject jObject, string binaryDataPath, string dataType, string id = null)
            => DataIoCommunicator.CreateSubmission(ApiConfiguration, httpClientProxy.Client, jObject, binaryDataPath, dataType, id);

        public Task TransferSubmissionData<T>(T obj, Func<T, byte[]> binaryDataPath, string id)
            => DataIoCommunicator.TransferSubmissionData<T>(ApiConfiguration, httpClientProxy.Client, obj, binaryDataPath, id);

        public Task TransferSubmissionData(JObject jObject, string binaryDataPath, string dataType, string id)
            => DataIoCommunicator.TransferSubmissionData(ApiConfiguration, httpClientProxy.Client, jObject, binaryDataPath, dataType, id);

        public Task TransferSubmissionData(string dataType, string id, byte[] binaryData)
            => DataIoCommunicator.TransferSubmissionData(ApiConfiguration, httpClientProxy.Client, dataType, id, new MemoryStream(binaryData));

        public Task TransferSubmissionData(string dataType, string id, Stream binaryData)
            => DataIoCommunicator.TransferSubmissionData(ApiConfiguration, httpClientProxy.Client, dataType, id, binaryData);

        public Task<DataReference> TransferFile(string filePath, string id = null)
            => DataTransferFunctions.TransferFile(this, filePath, id);

        public Task<DataReference> TransferBinaryData(byte[] data, string id = null)
            => DataTransferFunctions.TransferBinaryData(this, data, id);

        public Task<DataReference> TransferBinaryData(Stream data, string id = null)
            => DataTransferFunctions.TransferBinaryData(this, data, id);

        public Task<bool> ExistsAsync<T>(string id)
            => DataIoCommunicator.ExistsAsync(ApiConfiguration, httpClientProxy.Client, GetCollectionName<T>(), id);

        public Task<bool> ExistsAsync(string dataType, string id)
            => DataIoCommunicator.ExistsAsync(ApiConfiguration, httpClientProxy.Client, dataType, id);

        public Task<T> GetAsync<T>(string id)
            => DataIoCommunicator.GetAsync<T>(ApiConfiguration, httpClientProxy.Client, id);

        public Task<string> GetAsync(string dataType, string id)
            => DataIoCommunicator.GetAsync(ApiConfiguration, httpClientProxy.Client, dataType, id);

        public Task<T> GetSubmissionMetadata<T>(string id)
            => DataIoCommunicator.GetSubmissionMetadata<T>(ApiConfiguration, httpClientProxy.Client, id);

        public Task<string> GetSubmissionMetadata(string dataType, string id)
            => DataIoCommunicator.GetSubmissionMetadata(ApiConfiguration, httpClientProxy.Client, dataType, id);

        public Task<List<T>> GetManyAsync<T>(Expression<Func<T,bool>> filter, Expression<Func<T,object>> orderBy = null, uint? limit = null)
            => DataIoCommunicator.GetManyAsync<T>(ApiConfiguration, httpClientProxy.Client, filter != null ? expressionParser.ParseWhereExpression(filter) : null, orderBy != null ? expressionParser.ParseWhereExpression(orderBy) : null, limit);

        public Task<List<T>> GetManyAsync<T>(string whereArguments = null, string orderByArguments = null, uint? limit = null)
            => DataIoCommunicator.GetManyAsync<T>(ApiConfiguration, httpClientProxy.Client, whereArguments, orderByArguments, limit);

        public Task<List<string>> GetManyAsync(string dataType, string whereArguments, string orderByArguments = null, uint? limit = null)
            => DataIoCommunicator.GetManyAsync(ApiConfiguration, httpClientProxy.Client, dataType, whereArguments, orderByArguments, limit);

        public Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> orderBy = null)
            => DataIoCommunicator.FirstOrDefault<T>(ApiConfiguration, httpClientProxy.Client, filter != null ? expressionParser.ParseWhereExpression(filter) : null, orderBy != null ? expressionParser.ParseWhereExpression(orderBy) : null);

        public Task<T> FirstOrDefault<T>(string whereArguments, string orderByArguments = null)
            => DataIoCommunicator.FirstOrDefault<T>(ApiConfiguration, httpClientProxy.Client, whereArguments, orderByArguments);

        public Task<string> FirstOrDefault(string dataType, string whereArguments, string orderByArguments = null)
            => DataIoCommunicator.FirstOrDefault(ApiConfiguration, httpClientProxy.Client, dataType, whereArguments, orderByArguments);

        public Task DeleteAsync<T>(string id)
            => DataIoCommunicator.DeleteAsync(ApiConfiguration, httpClientProxy.Client, GetCollectionName<T>(), id);

        public Task DeleteAsync(string dataType, string id)
            => DataIoCommunicator.DeleteAsync(ApiConfiguration, httpClientProxy.Client, dataType, id);

        public Task<List<DeleteResult>> DeleteMany<T>(string whereArguments)
            => DataIoCommunicator.DeleteMany(ApiConfiguration, httpClientProxy.Client, GetCollectionName<T>(), whereArguments);

        public Task<List<DeleteResult>> DeleteMany(string dataType, string whereArguments)
            => DataIoCommunicator.DeleteMany(ApiConfiguration, httpClientProxy.Client, dataType, whereArguments);

        public Task<Stream> SearchAsync(string query, ResultFormat resultFormat)
            => DataIoCommunicator.SearchAsync(ApiConfiguration, httpClientProxy.Client, query, resultFormat);

        public void SetDataRedirection(string dataType, string dataSourceSystem)
            => DataIoCommunicator.SetDataRedirection(ApiConfiguration, httpClientProxy.Client, dataType, dataSourceSystem);

        public void SetCollectionOptions(CollectionOptions collectionOptions)
            => DataIoCommunicator.SetCollectionOptions(ApiConfiguration, httpClientProxy.Client, collectionOptions);

        public Task<CollectionInformation> GetCollectionInformationAsync(string collectionName)
            => DataIoCommunicator.GetCollectionInformationAsync(ApiConfiguration, httpClientProxy.Client, collectionName);

        public Task<List<string>> ListCollectionNamesAsync(bool includeHidden = false)
            => DataIoCommunicator.ListCollectionNamesAsync(ApiConfiguration, httpClientProxy.Client, includeHidden);

        public Task<List<CollectionInformation>> ListCollectionsAsync(bool includeHidden = false)
            => DataIoCommunicator.ListCollectionsAsync(ApiConfiguration, httpClientProxy.Client, includeHidden);
        #endregion


        #region ID
        public Task<string> GetNewIdAsync(string dataType)
            => IdCommunicator.GetIdAsync(ApiConfiguration, httpClientProxy.Client, dataType);
        public Task<string> GetNewIdAsync<T>()
            => IdCommunicator.GetIdAsync(ApiConfiguration, httpClientProxy.Client, GetCollectionName<T>());

        public Task<List<string>> GetMultipleNewIdsAsync(string dataType, uint count)
            => IdCommunicator.GetIdsAsync(ApiConfiguration, httpClientProxy.Client, dataType, count);

        public Task<List<string>> GetMultipleNewIdsAsync<T>(uint count)
            => IdCommunicator.GetIdsAsync(ApiConfiguration, httpClientProxy.Client, GetCollectionName<T>(), count);

        public Task ReserveIdAsync(string dataType, string id)
            => IdCommunicator.ReserveIdAsync(ApiConfiguration, httpClientProxy.Client, dataType, id);
        public Task ReserveIdAsync<T>(string id)
            => IdCommunicator.ReserveIdAsync(ApiConfiguration, httpClientProxy.Client, GetCollectionName<T>(), id);

        #endregion


        #region View
        public Task<ViewInformation> CreateViewAsync(string query, DateTime expires, string viewId = null)
            => ViewCommunicator.CreateViewAsync(ApiConfiguration, httpClientProxy.Client, query, expires, viewId);

        public Task<Stream> GetViewAsync(string viewId, ResultFormat resultFormat, Dictionary<string, string> parameters = null)
            => ViewCommunicator.GetViewAsync(ApiConfiguration, httpClientProxy.Client, viewId, resultFormat, parameters);

        public Task DeleteViewAsync(string viewId)
            => ViewCommunicator.DeleteViewAsync(ApiConfiguration, httpClientProxy.Client, viewId);
        #endregion


        #region Validator
        public Task SubmitValidatorAsync(ValidatorDefinition validatorDefinition, bool suppressAutoApprove = false)
            => ValidatorCommunicator.SubmitValidatorAsync(ApiConfiguration, httpClientProxy.Client, validatorDefinition, suppressAutoApprove);

        public Task ApplyValidatorAsync<T>(T obj, string validatorId = null)
            => ValidatorCommunicator.ApplyValidatorAsync(ApiConfiguration, httpClientProxy.Client, obj, validatorId);

        public Task ApplyValidatorAsync(string dataType, string json, string validatorId = null)
            => ValidatorCommunicator.ApplyValidatorAsync(ApiConfiguration, httpClientProxy.Client, dataType, json, validatorId);

        public Task<ValidatorDefinition> GetValidatorDefinitionAsync(string validatorId)
            => ValidatorCommunicator.GetValidatorDefinitionAsync(ApiConfiguration, httpClientProxy.Client, validatorId);

        public Task<List<ValidatorDefinition>> GetAllValidatorDefinitionsAsync(string dataType = null)
            => ValidatorCommunicator.GetAllValidatorDefinitionsAsync(ApiConfiguration, httpClientProxy.Client, dataType);

        public Task ApproveValidatorAsync(string validatorId)
            => ValidatorCommunicator.ApproveValidatorAsync(ApiConfiguration, httpClientProxy.Client, validatorId);

        public Task UnapproveValidatorAsync(string validatorId)
            => ValidatorCommunicator.UnapproveValidatorAsync(ApiConfiguration, httpClientProxy.Client, validatorId);

        public Task DeleteValidatorAsync(string validatorId)
            => ValidatorCommunicator.DeleteValidatorAsync(ApiConfiguration, httpClientProxy.Client, validatorId);
        #endregion


        #region Subscription
        public Task<string> SubscribeAsync(string dataType, IList<DataModificationType> modificationTypes, string filter = null)
            => SubscriptionCommunicator.SubscribeAsync(ApiConfiguration, httpClientProxy.Client, dataType, modificationTypes, filter);
        
        public Task UnsubscribeAsync(string subscriptionId)
            => SubscriptionCommunicator.UnsubscribeAsync(ApiConfiguration, httpClientProxy.Client, subscriptionId);

        public Task UnsubscribeAllAsync(string dataType)
            => SubscriptionCommunicator.UnsubscribeAllAsync(ApiConfiguration, httpClientProxy.Client, dataType);

        public Task<List<SubscriptionInfo>> GetSubscriptionsAsync(string dataType = null)
            => SubscriptionCommunicator.GetSubscriptionsAsync(ApiConfiguration, httpClientProxy.Client, dataType);

        public Task<List<SubscriptionNotification>> GetSubscribedObjects(string dataType = null)
            => SubscriptionCommunicator.GetSubscribedObjects(ApiConfiguration, httpClientProxy.Client, dataType);

        public Task DeleteNotificationAsync(string notificationId)
            => SubscriptionCommunicator.DeleteNotificationAsync(ApiConfiguration, httpClientProxy.Client, notificationId);

        public Task ReportTo(string recipient, string dataType, string dataObjectId)
            => SubscriptionCommunicator.ReportTo(ApiConfiguration, httpClientProxy.Client, recipient, dataType, dataObjectId);
        #endregion


        #region High-level functions

        public Task CreateDataCollectionProtocol(string protocolName, List<DataCollectionProtocolParameter> parameters, List<DataPlaceholder> expectedData)
            => DataProjectFunctions.CreateDataCollectionProtocol(this, protocolName, parameters, expectedData);

        public Task CreateDataProject(string dataProjectId, IdSourceSystem projectSourceSystem, string protocolName, Dictionary<string, string> protocolParameterResponses)
            => DataProjectFunctions.CreateDataProject(this, dataProjectId, projectSourceSystem, protocolName, protocolParameterResponses);

        public Task AddToDataProject(string dataProjectId, string dataType, string id, List<DataReference> derivedData, string filename)
            => DataProjectFunctions.AddToDataProject(this, dataProjectId, dataType, id, derivedData, filename);

        public Task AddToDataSet(string dataSetId, string dataType, string id)
            => DataProjectFunctions.AddToDataSet(this, dataSetId, dataType, id);

        #endregion


        #region Type to collection mapping

        public static void RegisterType<T>(string collectionName)
        {
            RegisterType(typeof(T), collectionName);
        }
        public static void RegisterType(Type type, string collectionName)
        {
            CollectionNameDeterminer.TypeMap[type] = collectionName;
        }

        public static void UnregisterType<T>()
        {
            UnregisterType(typeof(T));
        }
        public static void UnregisterType(Type type)
        {
            CollectionNameDeterminer.TypeMap.Remove(type);
        }

        public static string GetCollectionName<T>()
        {
            return GetCollectionName(typeof(T));
        }
        public static string GetCollectionName(Type type)
        {
            return CollectionNameDeterminer.GetCollectionName(type);
        }

        #endregion
    }
}
