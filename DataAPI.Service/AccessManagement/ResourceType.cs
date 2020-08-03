namespace DataAPI.Service.AccessManagement
{
    public enum ResourceType
    {
        // Data IO
        SubmitData = 1,
        GetData = 2,
        DeleteData = 3,
        Search = 4,
        ProtectCollection = 5,
        ListCollections = 6,
        ViewCollectionInformation = 7,
        SetDataRedirection = 8,
        SetCollectionOptions = 9,

        // Views
        CreateView = 101,
        GetView = 102,
        DeleteView = 103,

        // Validators
        AddValidator = 201,
        GetValidator = 202,
        ManageValidators = 203,

        // User management
        ManageUser = 301,
        ViewUserProfiles = 302,
        GetCollectionPermissions = 303,
        GetGlobalRoles = 304,
        
        // Subscription
        SubscribeToData = 401,
        DeleteNotification = 402,
        ReportData = 403,
        ListSubscriptions = 404,
        Unsubscribe = 405
    }
}