using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;
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
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedMemberInSuper.Global
    public interface IDataApiClient
    {
        ApiConfiguration ApiConfiguration { get; }

        LoginMethod LoginMethod { get; set; }
        bool IsLoggedIn { get; }
        string LoggedInUsername { get; }

        /// <summary>
        /// Checks status of the API server.
        /// </summary>
        /// <returns>True if API is reachable, false if any error occurs</returns>
        bool IsAvailable();

        /// <summary>
        /// Get user profiles of all registered users
        /// </summary>
        Task<List<UserProfile>> GetAllUserProfiles();

        /// <summary>
        /// Get global roles for user
        /// </summary>
        Task<List<Role>> GetGlobalRolesForUser(string username);

        /// <summary>
        /// Login using Active Directory
        /// </summary>
        /// <returns>Object containing flag indicating whether or not the login was successful and contains the access token</returns>
        AuthenticationResult Login();

        /// <summary>
        /// Login using user-password-combination
        /// </summary>
        /// <returns>Object containing flag indicating whether or not the login was successful and contains the access token</returns>
        AuthenticationResult Login(string username, string password);

        /// <summary>
        /// After calling <see cref="Login(string, string)"/> once, this method can be used to login again, e.g. after the connection was interupted.
        /// </summary>
        /// <returns>Object containing flag indicating whether or not the login was successful and contains the access token</returns>
        AuthenticationResult RetryLogin();

        /// <summary>
        /// Set access token, which typically is obtained from other sources.
        /// Example: A user uses a website, where the web server accesses
        /// the DataAPI using the access token provided by the user.
        /// </summary>
        void SetAccessToken(string accessToken);

        /// <summary>
        /// Invalidate the provided access token, such that it cannot be used for other requests.
        /// </summary>
        void Logout();

        /// <summary>
        /// Register a new user. Password must not be empty. If user already exists an exception is thrown.
        /// </summary>
        void Register(string username, string firstName, string lastName, string password, string email);

        /// <summary>
        /// Change password of user
        /// </summary>
        void ChangePassword(string username, string password);

        /// <summary>
        /// Add global role to user (requires role as UserManager or Admin).
        /// Users cannot assign roles to themself.
        /// Global roles are applied to unprotected collections.
        /// </summary>
        void AddGlobalRoleToUser(string username, Role role);

        /// <summary>
        /// Add collection role to user (requires role as UserManager or Admin).
        /// Users cannot assign roles to themself.
        /// Collection roles are applied to protected collections.
        /// </summary>
        void AddCollectionRoleToUser(string username, Role role, string dataType);

        /// <summary>
        /// Set global roles for user. Overwrites any existing roles.
        /// Setting roles requires role as UserManger or Admin)
        /// Users cannot assign roles to themself.
        /// Global roles are applied to unprotected collections.
        /// </summary>
        void SetGlobalRolesForUser(string username, IList<Role> roles);

        /// <summary>
        /// Set collection roles for user. Overwrites any existing roles.
        /// Setting roles requires role as UserManger or Admin)
        /// Users cannot assign roles to themself.
        /// Collection roles are applied to protected collections.
        /// </summary>
        void SetCollectionRoleForUser(string username, IList<Role> roles, string dataType);

        /// <summary>
        /// Remove global role from user (requires role as UserManager or Admin).
        /// Users cannot assign roles to themself.
        /// Global roles are applied to unprotected collections.
        /// </summary>
        void RemoveGlobalRoleFromUser(string username, Role role);

        /// <summary>
        /// Remove collection role from user (requires role as UserManager or Admin).
        /// Users cannot assign roles to themself.
        /// Collection roles are applied to protected collections.
        /// </summary>
        void RemoveCollectionRoleFromUser(string username, Role role, string dataType);

        /// <summary>
        /// Remove user. Users can remove themselves.
        /// </summary>
        void DeleteUser(string username);

        /// <summary>
        /// Get list of user permissions for collection. List is empty is collection is not protected.
        /// Requires UserManager or Admin role on collection to get list.
        /// </summary>
        Task<List<CollectionUserPermissions>> GetCollectionPermissions(string dataType);

        /// <summary>
        /// Insert <paramref name="obj"/> into database. The <paramref name="id"/> to be used can be specified, but will throw an exception if object with this ID already exists.
        /// </summary>
        /// <returns>ID of inserted object</returns>
        Task<string> InsertAsync(object obj, string id = null);

        /// <summary>
        /// Insert <paramref name="json"/> of type <paramref name="dataType"/> into database. The <paramref name="id"/> to be used can be specified, but will throw an exception if object with this ID already exists.
        /// </summary>
        /// <returns>ID of inserted object</returns>
        Task<string> InsertAsync(string dataType, string json, string id = null);

        /// <summary>
        /// Insert or replace <paramref name="obj"/> into database.
        /// </summary>
        /// <returns>ID of inserted or replaced object</returns>
        Task<string> ReplaceAsync(object obj, string existingId);

        /// <summary>
        /// Insert or replace <paramref name="json"/> of type <paramref name="dataType"/> into database.
        /// </summary>
        /// <returns>ID of inserted or replaced object</returns>
        Task<string> ReplaceAsync(string dataType, string json, string existingId);

        /// <summary>
        /// For large data objects like files > 10 MB.
        /// Like <see cref="InsertAsync(object,string)"/> but the binary data will be stripped.
        /// Use <see cref="TransferSubmissionData{T}(T,Func{T,byte[]},string)"/> to transfer the binary data.
        /// </summary>
        /// <param name="obj">Object to be submitted</param>
        /// <param name="binaryDataPath">Path to binary data</param>
        /// <param name="id">Suggested ID of submission. This ID might be overruled by DataAPI</param>
        /// <returns>Actual ID of submission as determined by DataAPI</returns>
        Task<string> CreateSubmission<T>(T obj, Func<T, byte[]> binaryDataPath, string id = null);

        /// <summary>
        /// For large data objects like files > 10 MB.
        /// Like <see cref="InsertAsync(string,string,string)"/> but the binary data will be stripped.
        /// Use <see cref="TransferSubmissionData(string,string,byte[])"/> to transfer the binary data.
        /// </summary>
        /// <param name="jObject">Object to be submitted</param>
        /// <param name="binaryDataPath">Path to binary data</param>
        /// <param name="dataType">Data type</param>
        /// <param name="id">Suggested ID of submission. This ID might be overruled by DataAPI</param>
        /// <returns>Actual ID of submission as determined by DataAPI</returns>
        Task<string> CreateSubmission(JObject jObject, string binaryDataPath, string dataType, string id = null);

        /// <summary>
        /// Transfer binary data for submission created with <see cref="CreateSubmission{T}"/>
        /// </summary>
        /// <param name="obj">Object to be submitted</param>
        /// <param name="binaryDataPath">Path to binary data</param>
        /// <param name="id">ID of submission</param>
        Task TransferSubmissionData<T>(T obj, Func<T, byte[]> binaryDataPath, string id);

        /// <summary>
        /// Transfer binary data for submission created with <see cref="CreateSubmission(JObject,string,string,string)"/>
        /// </summary>
        /// <param name="jObject">Object to be submitted</param>
        /// <param name="binaryDataPath">Path to binary data</param>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID of submission</param>
        Task TransferSubmissionData(JObject jObject, string binaryDataPath, string dataType, string id);

        /// <summary>
        /// Transfer binary data for submission created with <see cref="CreateSubmission"/>
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID of submission</param>
        /// <param name="binaryData">Binary data to be transferred</param>
        Task TransferSubmissionData(string dataType, string id, byte[] binaryData);

        /// <summary>
        /// Transfer stream of data for submission created with <see cref="CreateSubmission"/>
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="id">ID of submission</param>
        /// <param name="stream">Stream to be transferred</param>
        Task TransferSubmissionData(string dataType, string id, Stream stream);

        /// <summary>
        /// Transfer a file. Will be stored as a DataBlob.
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="id">Suggested ID. May be overruled by DataAPI</param>
        /// <returns>Reference to uploaded file</returns>
        Task<DataReference> TransferFile(string filePath, string id = null);

        /// <summary>
        /// Transfer binary data. Will be stored as a DataBlob.
        /// </summary>
        /// <param name="data">Binary data to be uploaded</param>
        /// <param name="id">Suggested ID. May be overruled by DataAPI</param>
        /// <returns>Reference to uploaded file</returns>
        Task<DataReference> TransferBinaryData(byte[] data, string id = null);

        /// <summary>
        /// Transfer binary data. Will be stored as a DataBlob.
        /// </summary>
        /// <param name="data">Binary data to be uploaded</param>
        /// <param name="id">Suggested ID. May be overruled by DataAPI</param>
        /// <returns>Reference to uploaded file</returns>
        Task<DataReference> TransferBinaryData(Stream data, string id = null);

        /// <summary>
        /// Check existance of object with the given ID.
        /// </summary>
        /// <returns>True, if object exists, false if not. Throws exception if other status code</returns>
        Task<bool> ExistsAsync<T>(string id);

        /// <summary>
        /// Check existance of object with the given ID.
        /// </summary>
        /// <returns>True, if object exists, false if not. Throws exception if other status code</returns>
        Task<bool> ExistsAsync(string dataType, string id);

        /// <summary>
        /// Retrieves object of the specified type <typeparamref name="T"/> and <paramref name="id"/> from database.
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve from database</typeparam>
        Task<T> GetAsync<T>(string id);

        /// <summary>
        /// Retrieves JSON representation of object of the specified type <paramref name="dataType"/> and <paramref name="id"/> from database.
        /// </summary>
        Task<string> GetAsync(string dataType, string id);

        /// <summary>
        /// Like <see cref="GetAsync{T}"/> but if the object requested has binary payload, e.g. DataBlob or Image, the payload is not included in the response.
        /// </summary>
        Task<T> GetSubmissionMetadata<T>(string id);

        /// <summary>
        /// Like <see cref="GetAsync"/> but if the object requested has binary payload, e.g. DataBlob or Image, the payload is not included in the response.
        /// </summary>
        Task<string> GetSubmissionMetadata(string dataType, string id);

        /// <summary>
        /// Retrieve objects matching the filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter">Filter</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="limit">Maximum number of items to be returned</param>
        /// <returns>List of matching objects</returns>
        Task<List<T>> GetManyAsync<T>(Expression<Func<T,bool>> filter, Expression<Func<T,object>> orderBy = null, uint? limit = null);

        /// <summary>
        /// Retrieve objects matching the filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereArguments">
        /// Filter in the same format as accepted by the WHERE-clause of SQL-like queries (but without WHERE),
        /// e.g. Data.ProductName = 'Test' AND Data.Price > 200
        /// </param>
        /// <param name="orderByArguments">
        /// Order by statement in the same format accepted by ORDER BY-clause in SQL-like queries (but without ORDER BY)
        /// E.g. Data.Age DESC, Data.Name ASC
        /// </param>
        /// <param name="limit">Maximum number of items to be returned</param>
        /// <returns>List of matching objects</returns>
        Task<List<T>> GetManyAsync<T>(string whereArguments = null, string orderByArguments = null, uint? limit = null);

        /// <summary>
        /// Retrieve objects matching the filter
        /// </summary>
        /// <param name="dataType">Name of collection</param>
        /// <param name="whereArguments">
        /// Filter in the same format as accepted by the WHERE-clause of SQL-like queries (but without WHERE),
        /// e.g. Data.ProductName = 'Test' AND Data.Price > 200
        /// </param>
        /// <param name="orderByArguments">
        /// Order by statement in the same format accepted by ORDER BY-clause in SQL-like queries (but without ORDER BY)
        /// E.g. Data.Age DESC, Data.Name ASC
        /// </param>
        /// <param name="limit">Maximum number of items to be returned</param>
        /// <returns>List of JSON representation of matching objects</returns>
        Task<List<string>> GetManyAsync(string dataType, string whereArguments = null, string orderByArguments = null, uint? limit = null);

        /// <summary>
        /// Get first instance matching the filter
        /// </summary>
        Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> filter, Expression<Func<T,object>> orderBy = null);

        /// <summary>
        /// Get first instance matching the filter
        /// </summary>
        /// <param name="whereArguments">
        /// Filter in the same format as accepted by the WHERE-clause of SQL-like queries (but without WHERE),
        /// e.g. Data.ProductName = 'Test' AND Data.Price > 200
        /// </param>
        /// <param name="orderByArguments">
        /// Order by statement in the same format accepted by ORDER BY-clause in SQL-like queries (but without ORDER BY)
        /// E.g. Data.Age DESC, Data.Name ASC
        /// </param>
        Task<T> FirstOrDefault<T>(string whereArguments, string orderByArguments = null);

        /// <summary>
        /// Get JSON of first instance matching the filter
        /// </summary>
        /// <param name="dataType">Name of collection</param>
        /// <param name="whereArguments">
        /// Filter in the same format as accepted by the WHERE-clause of SQL-like queries (but without WHERE),
        /// e.g. Data.ProductName = 'Test' AND Data.Price > 200
        /// </param>
        /// <param name="orderByArguments">
        /// Order by statement in the same format accepted by ORDER BY-clause in SQL-like queries (but without ORDER BY)
        /// E.g. Data.Age DESC, Data.Name ASC
        /// </param>
        Task<string> FirstOrDefault(string dataType, string whereArguments, string orderByArguments = null);

        /// <summary>
        /// Delete object of the specified type <typeparamref name="T"/> and <paramref name="id"/> from database.
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve from database</typeparam>
        Task DeleteAsync<T>(string id);

        /// <summary>
        /// Delete object of the type <paramref name="dataType"/> and <paramref name="id"/> from database.
        /// </summary>
        Task DeleteAsync(string dataType, string id);

        /// <summary>
        /// Delete all items matching filter
        /// </summary>
        /// <param name="whereArguments">
        /// Filter in the same format as accepted by the WHERE-clause of SQL-like queries (but without WHERE),
        /// e.g. Data.ProductName = 'Test' AND Data.Price > 200
        /// </param>
        /// <returns>Number of deleted items</returns>
        Task<List<DeleteResult>> DeleteMany<T>(string whereArguments);

        /// <summary>
        /// Delete all items matching filter
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="whereArguments">
        ///     Filter in the same format as accepted by the WHERE-clause of SQL-like queries (but without WHERE),
        ///     e.g. Data.ProductName = 'Test' AND Data.Price > 200
        /// </param>
        /// <returns>Number of deleted items</returns>
        Task<List<DeleteResult>> DeleteMany(string dataType, string whereArguments);

        /// <summary>
        /// Run SQL-like <paramref name="query"/> and return results in the specified <paramref name="resultFormat"/>.
        /// </summary>
        /// <returns>Stream of results. Each line contains one JSON object. Use <see cref="SeachResultStreamExtensions.ReadAllSearchResultsAsync"/> which implements the necessary parsing</returns>
        Task<Stream> SearchAsync(string query, ResultFormat resultFormat);

        /// <summary>
        /// Set the data system to which a data type is redirected.
        /// Requires global Admin-role.
        /// </summary>
        /// <param name="dataType">Name of data type</param>
        /// <param name="dataSourceSystem">Data source system. 2019-08-29: Available: MongoDB, FileSystem, AzureBlobStorage, ExistingSQL, GenericSQL</param>
        void SetDataRedirection(string dataType, string dataSourceSystem);

        /// <summary>
        /// Set collection options, like display name, protection status, overwrite permissions, etc.
        /// </summary>
        void SetCollectionOptions(CollectionOptions collectionOptions);

        /// <summary>
        /// List detailed information about all collections
        /// </summary>
        Task<CollectionInformation> GetCollectionInformationAsync(string collectionName);

        /// <summary>
        /// List names of all collections
        /// </summary>
        Task<List<string>> ListCollectionNamesAsync(bool includeHidden = false);

        /// <summary>
        /// List detailed information about all collections
        /// </summary>
        Task<List<CollectionInformation>> ListCollectionsAsync(bool includeHidden = false);

        /// <summary>
        /// Create a view/stored query
        /// </summary>
        /// <param name="query">
        /// The query used to create the view. Supports placeholders of the form {parameter}.
        /// Placeholders are filled in by the key-value-pairs provided by the <see cref="DataApiClient.GetViewAsync"/> request
        /// </param>
        /// <param name="expires">Expiration date of the view.</param>
        /// <param name="viewId">ID of the view to create</param>
        /// <returns>Object containing the view-ID</returns>
        Task<ViewInformation> CreateViewAsync(string query, DateTime expires, string viewId = null);

        /// <summary>
        /// Get the entries in the view
        /// </summary>
        /// <param name="viewId">ID of the view</param>
        /// <param name="parameters">Key-value-pairs to fill in placeholders in the view-query</param>
        /// <param name="resultFormat">Format of the result (e.g. JSON or CSV)</param>
        /// <returns>View results. See <seealso cref="DataApiClient.SearchAsync"/></returns>
        Task<Stream> GetViewAsync(string viewId, ResultFormat resultFormat, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Get the entries in the view
        /// </summary>
        /// <param name="viewId">ID of the view</param>
        Task DeleteViewAsync(string viewId);

        /// <summary>
        /// Submit new validator. If validator returns 'true' for all existing data it will be auto-approved, else needs approval by an administrator.
        /// </summary>
        /// <returns>A task which can be used to check if the submission was successful.</returns>
        Task SubmitValidatorAsync(ValidatorDefinition validatorDefinition, bool suppressAutoApprove = false);

        /// <summary>
        /// If <paramref name="validatorId"/> is specified, applies this validator to the data object.
        /// Otherwise applies all approved validators.
        /// </summary>
        /// <returns>Throws exception with BadRequest-status code if data doesn't match validator</returns>
        Task ApplyValidatorAsync<T>(T obj, string validatorId = null);

        /// <summary>
        /// If <paramref name="validatorId"/> is specified, applies this validator to the data object.
        /// Otherwise applies all approved validators.
        /// </summary>
        /// <returns>Throws exception with BadRequest-status code if data doesn't match validator</returns>
        Task ApplyValidatorAsync(string dataType, string json, string validatorId = null);

        /// <summary>
        /// Get validator definition for validator with the provided ID. Non-admins can only access validator definitions they submitted themselves.
        /// </summary>
        /// <returns>Validator definition with matching ID on success, otherwise the provided task fails</returns>
        Task<ValidatorDefinition> GetValidatorDefinitionAsync(string validatorId);

        /// <summary>
        /// (Admins only) Get all validator definitions.
        /// If <paramref name="dataType"/> is specified, only returns validators for that collection.
        /// </summary>
        /// <returns>List of validator definitions</returns>
        Task<List<ValidatorDefinition>> GetAllValidatorDefinitionsAsync(string dataType = null);

        /// <summary>
        /// (Admins only) Approve validator with provided ID
        /// </summary>
        /// <returns>A task which can be used to check if the submission was successful.</returns>
        Task ApproveValidatorAsync(string validatorId);

        /// <summary>
        /// (Admins only) Unapprove validator with provided ID
        /// </summary>
        /// <returns>A task which can be used to check if the submission was successful.</returns>
        Task UnapproveValidatorAsync(string validatorId);

        /// <summary>
        /// (Admins only) Remove validator with provided ID
        /// </summary>
        /// <returns>A task which can be used to check if the submission was successful.</returns>
        Task DeleteValidatorAsync(string validatorId);

        /// <summary>
        /// Subscribe to data with options for filtering
        /// what modification has happened (created, replaced, deleted)
        /// and providing an expression for filtering data.
        /// </summary>
        Task<string> SubscribeAsync(string dataType, IList<DataModificationType> modificationTypes, string filter = null);

        /// <summary>
        /// Unsubscribe from a specific subscription
        /// </summary>
        Task UnsubscribeAsync(string subscriptionId);

        /// <summary>
        /// Unsubscribe all subscriptions for a specific data type
        /// </summary>
        Task UnsubscribeAllAsync(string dataType);

        /// <summary>
        /// Get all subscriptions for the logged in user.
        /// If <paramref name="dataType"/> is specified, only subscriptions for that data type are returned.
        /// </summary>
        Task<List<SubscriptionInfo>> GetSubscriptionsAsync(string dataType = null);

        /// <summary>
        /// Get notifications for all data changes matching the logged in user's subscriptions.
        /// </summary>
        Task<List<SubscriptionNotification>> GetSubscribedObjects(string dataType = null);

        /// <summary>
        /// Delete notification from notification list
        /// </summary>
        Task DeleteNotificationAsync(string notificationId);

        /// <summary>
        /// Report data to a specific user
        /// </summary>
        /// <param name="recipient">Username of the recipient (case sensitive)</param>
        /// <param name="dataType">Type of data to report</param>
        /// <param name="dataObjectId">ID of data object</param>
        Task ReportTo(string recipient, string dataType, string dataObjectId);

        /// <summary>
        /// Get ID for <paramref name="dataType"/>
        /// </summary>
        /// <param name="dataType">Name of data type, e.g. Trial, Batch, Plate, Image, etc.</param>
        /// <returns>A globally unique ID</returns>
        Task<string> GetNewIdAsync(string dataType);

        /// <summary>
        /// Get ID for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The data type for which an ID is requested</typeparam>
        /// <returns>A globally unique ID</returns>
        Task<string> GetNewIdAsync<T>();

        /// <summary>
        /// Get several IDs for <paramref name="dataType"/>
        /// </summary>
        /// <param name="dataType">Name of data type, e.g. Trial, Batch, Plate, Image, etc.</param>
        /// <param name="count">Number of IDs</param>
        /// <returns>Unique reserved IDs</returns>
        Task<List<string>> GetMultipleNewIdsAsync(string dataType, uint count);

        /// <summary>
        /// Get several IDs for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The data type for which an ID is requested</typeparam>
        /// <param name="count">Number of IDs</param>
        /// <returns>Unique reserved IDs</returns>
        Task<List<string>> GetMultipleNewIdsAsync<T>(uint count);

        /// <summary>
        /// Reserve ID for datatype <paramref name="dataType"/>
        /// </summary>
        /// <param name="dataType">Name of data type, e.g. Trial, Batch, Plate, Image, etc.</param>
        /// <param name="id">The ID to be reserved</param>
        /// <returns>Nothing, but throws exception if ID cannot be reserved</returns>
        Task ReserveIdAsync(string dataType, string id);

        /// <summary>
        /// Reserve ID for datatype <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The data type for which an ID is requested</typeparam>
        /// <returns>Nothing, but throws exception if ID cannot be reserved</returns>
        Task ReserveIdAsync<T>(string id);

        /// <summary>
        /// Create data collection protocol.
        /// This protocol describes the data that must be collected when following this protocol,
        /// Example: When following the "Cake baking"-protocol it could be mandatory to provide
        /// Parameters:
        /// - the name of the case
        /// - who baked it
        /// - link to the recipe
        /// Expected data:
        /// - image of the cake
        /// </summary>
        /// <param name="protocolName">Name of the protocol</param>
        /// <param name="parameters">List of parameters</param>
        /// <param name="expectedData">List of expected data objects</param>
        Task CreateDataCollectionProtocol(string protocolName, List<DataCollectionProtocolParameter> parameters, List<DataPlaceholder> expectedData);

        /// <summary>
        /// Create a data project implementing a data collection protocol
        /// </summary>
        /// <param name="dataProjectId">ID of the data project</param>
        /// <param name="projectSourceSystem">Source system of the project (where was the project first registered?)</param>
        /// <param name="protocolName">Data collection protocol name <seealso cref="CreateDataCollectionProtocol"/> </param>
        /// <param name="protocolParameterResponses">
        /// Responses to the parameters of the data collection protocol.
        /// At least all mandatory parameters must be answered, otherwise an exception is thrown
        /// </param>
        Task CreateDataProject(string dataProjectId, string projectSourceSystem, string protocolName, Dictionary<string, string> protocolParameterResponses);

        /// <summary>
        /// Add object to data project. Throws exception if data project doesn't exist
        /// </summary>
        /// <param name="dataProjectId">ID of data project</param>
        /// <param name="dataType">Data type of object to add</param>
        /// <param name="id">ID of object to add</param>
        /// <param name="derivedData">List of data derived from the reference data object</param>
        /// <param name="filename">Filename of data object (if it's a file)</param>
        Task AddToDataProject(string dataProjectId, string dataType, string id, List<DataReference> derivedData = null, string filename = null);


        /// <summary>
        /// Add data object to data set, or equivalent: Tag data object
        /// </summary>
        /// <param name="dataSetId">Data set ID / tag name</param>
        /// <param name="dataType">Data type of object</param>
        /// <param name="id">ID of object</param>
        Task AddToDataSet(string dataSetId, string dataType, string id);
    }
}