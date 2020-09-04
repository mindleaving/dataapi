# dataapi
Data storage API with data validation and automatic processing

The DataAPI is a web-API that is a single and uniform entry point to data stored in different locations. It hides away the underlying infrastructure and integrations from users and makes data accessible through a single point of entry. It supports storage of any data that can be represented as JSON.

# Getting started
## Solutions
- **DataAPI.sln**: Contains the API, DataAPI data structures and the .NET client.

## Prerequisites
Before being able to start your DataAPI, you need:
- Visual Studio 2019
- .NET Core 3.1 SDK or newer
- MongoDB
  - [Install MongoDB](https://www.mongodb.com/try/download/community)
  - [Setup as a replica set](https://docs.mongodb.com/manual/tutorial/deploy-replica-set/) 
Optional, if not done change DataAPI.Web > appsettings.conf > MongoDB > ReplicaSetName to "null"
  - [Setup authentication](https://docs.mongodb.com/manual/tutorial/enable-authentication/)
  - [Create a user and assign](https://docs.mongodb.com/manual/tutorial/enable-authentication/#create-additional-users-as-needed-for-your-deployment) that user "dbOwner"-permissions to these three databases: **ApiAuthentication**, **DataApiBackendData** and **RD** (databases are automatically created in MongoDB when the first collection is created, they don't need to exist when assigning permissions to a user. When running the DataAPI for the first time these databases and their collections are automatically created).
  - (optional) [Install Robo3T](https://robomongo.org/) and connect to your MongoDB using the admin user created during "Setup authentication". If you want to connect with DataAPI-user you need to add "listDatabases" permissions on the "admin"-database to that user, otherwise you will see an error stating that you don't have permissions to list databases.
- Set environment variables
  - DataAPI_MongoDbPassword: <password for the MongoDB user created above>
  - See DataAPI.Web > appsettings.json for more environment variable names that should be set to appropriate values.
  - Note console output first time you start the DataAPI. If you haven't set the DataAPI_JwtSigningPrivateKey environment variable a message with a suggested signing key is printed. To suppress that message an keep access tokens valid across restarts of the webserver create the DataAPI_JwtSigningPrivateKey with the suggested value.
- Adjust DataAPI.Web > appsettings.json according to your needs and the desired logins. If you used the suggested database names for MongoDB above, set MongoDB.DataDatabaseName to "RD", MongoDB.BackendDataDatabaseName to "DataApiBackendData" and MongoDB.AuthorizationDatabaseName to "ApiAuthentication". Note: MongoDB.AuthorizationServer is not used.
- If you change DataAPI.Client or DataAPI.DataStructures you need to place the updated NuGet packages in a local folder (or other NuGet source) and add that folder as a NuGet package source in Visual Studio.

## Webserver setup
- Install IIS on the server if not installed by default
- [Install ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/permalink/dotnetcore-current-windows-runtime-bundle-installer)
- Publish DataAPI.Web to a folder and copy these files to the webserver (or publish directly to the server if you know how to set that up).
- Get a SSL-certificate for the website and install it
- Create a new website in IIS which points to the DataAPI.Web-files and configure it for SSL with the installed IIS certificate
- Start the website
- Test that the website was started by visiting \<website address\>/api/servicestatus/ping
- Test the DataAPI-functionality by running all tests in DataAPI.IntegrationTest (NB: set address to website in ApiSetup.cs)
