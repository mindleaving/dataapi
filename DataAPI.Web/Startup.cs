using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Commons.Misc;
using DataAPI.DataStructures;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.SubscriptionManagement;
using DataAPI.Service.Validators;
using DataAPI.Web.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

#pragma warning disable 1591

namespace DataAPI.Web
{
    public class Startup
    {
        private const string CorsPolicyName = "LocalhostCorsPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, builder =>
                {
                    var origins = Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
                    builder.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            services.AddControllers(
                options =>
                {
                    options.Filters.Add(new ProducesAttribute(Conventions.JsonContentType));
                })
                .AddNewtonsoftJson();

            SetupJwtTokenAuthentication(services);

            var dbUserName = Configuration["MongoDB:Username"];
            var password = Secrets.Get(Configuration["MongoDB:PasswordEnvironmentVariable"]);
            var dataServerAddresses = Configuration["MongoDB:Servers"].Split(',');
            var replicaSetName = Configuration["MongoDB:ReplicaSetName"];
            var dataDatabaseName = Configuration["MongoDB:DataDatabaseName"];
            var backendDataDatabaseName = Configuration["MongoDB:BackendDataDatabaseName"];
            var mailServerAddress = Configuration["Mail:ServerAddress"];
            var mailServerPort = ushort.Parse(Configuration["Mail:ServerPort"]);
            var mailFromAddress = Configuration["Mail:From"];
            var mailUseSSL = bool.Parse(Configuration["Mail:UseSSL"]);
            var mailAuthenticationRequired = bool.Parse(Configuration["Mail:UseSSL"]);
            var mailPasswordEnvironmentVariableName = Configuration["Mail:PasswordEnvironmentVariable"];
            var rdDataMongoClient = new RdDataMongoClient(dataServerAddresses, replicaSetName, dataDatabaseName, backendDataDatabaseName, dbUserName, password);

            var authorizationDatabaseName = Configuration["MongoDB:AuthorizationDatabaseName"];
            var accessControlMongoClient = new AccessControlMongoClient(dataServerAddresses, replicaSetName, authorizationDatabaseName, dbUserName, password);


            services.AddSingleton(accessControlMongoClient);
            services.AddSingleton<AuthenticationModule>(); // Depends on JWT token setup
            services.AddSingleton<IIdPolicy>(new DefaultIdPolicy());
            var apiEventLogger = new ApiEventLogger(rdDataMongoClient);
            if (string.IsNullOrEmpty(mailServerAddress))
            {
                services.AddSingleton<IMailSender>(new NoMailSender());
            }
            else
            {
                // Test mail password availability. Will throw exception if it doesn't exist
                Secrets.Get(mailPasswordEnvironmentVariableName);
                var mailSender = new MailSender(
                    mailServerAddress,
                    mailServerPort,
                    mailFromAddress,
                    mailUseSSL,
                    mailAuthenticationRequired,
                    mailPasswordEnvironmentVariableName,
                    apiEventLogger);
                services.AddSingleton<IMailSender>(mailSender);
            }
            services.AddSingleton<NewCollectionTasks>();


            var idGeneratorManager = new IdGeneratorManager(rdDataMongoClient);
            var rdDataStorages = new List<IRdDataStorage>
            {
                new MongoDbRdDataStorage(DataStorageTypes.MongoDB, rdDataMongoClient, idGeneratorManager)
            };

            SetupFileSystemStorages(rdDataStorages, idGeneratorManager);
            SetupAzureBlobStorages(rdDataStorages, idGeneratorManager);
            SetupSqlStorages(rdDataStorages, idGeneratorManager);

            var authorizationModule = new AuthorizationModule(accessControlMongoClient, rdDataMongoClient);
            var collectionInformationManager = new CollectionInformationManager(rdDataMongoClient, authorizationModule);
            var dataRouter = new DataRouter(rdDataMongoClient, DataStorageTypes.MongoDB, rdDataStorages);
            var validatorFactory = new ValidatorFactory(dataRouter);
            services.AddSingleton(authorizationModule);
            services.AddSingleton(collectionInformationManager);
            services.AddSingleton(apiEventLogger);
            services.AddSingleton(rdDataStorages);
            services.AddSingleton(new ViewManager(rdDataMongoClient));
            services.AddSingleton(new ValidatorManager(rdDataMongoClient, validatorFactory));
            services.AddSingleton(new SubscriptionManager(rdDataMongoClient, dataRouter));
            services.AddSingleton<IDataRouter>(dataRouter);

            services.AddSwaggerGen(c => 
            { 
                c.SwaggerDoc($"v{ApiVersion.Current}", new OpenApiInfo
                {
                    Title = "DataAPI", 
                    Version = $"v{ApiVersion.Current}"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "frontend/build";
            });

        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                TypescriptGeneration.Generate();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(
                c =>
                {
                    c.SwaggerEndpoint($"./v{ApiVersion.Current}/swagger.json", "DataAPI");
                    //c.RoutePrefix = null;
                });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors(CorsPolicyName);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "frontend";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000/");
                }
            });
        }

        private void SetupJwtTokenAuthentication(IServiceCollection services)
        {
            var jwtPrivateKeyEnvironmentVariable = Configuration["Authentication:Jwt:PrivateKeyEnvironmentVariable"];
            SymmetricSecurityKey privateKey;
            try
            {
                privateKey = new SymmetricSecurityKey(Convert.FromBase64String(Secrets.Get(jwtPrivateKeyEnvironmentVariable)));
            }
            catch (KeyNotFoundException)
            {
                using var rng = new RNGCryptoServiceProvider();
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                privateKey = new SymmetricSecurityKey(bytes);
                Console.WriteLine(
                    $"JWT private key candidate: {Convert.ToBase64String(bytes)}. Store this as environment variable '{jwtPrivateKeyEnvironmentVariable}'.");
            }

            services.AddAuthentication(
                    x =>
                    {
                        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = privateKey,
                            ValidateIssuer = false, // TODO Change after move to production environment
                            ValidateAudience = false // TODO Change after move to production environment
                        };
                    });
            var jwtTokenBuilder = new JwtSecurityTokenBuilder(privateKey, TimeSpan.FromMinutes(60));
            services.AddSingleton<ISecurityTokenBuilder>(jwtTokenBuilder);
        }

        private void SetupFileSystemStorages(List<IRdDataStorage> rdDataStorages, IdGeneratorManager idGeneratorManager)
        {
            var fileSystemStoragePath = Configuration["FileSystem:Path"];
            if (!string.IsNullOrEmpty(fileSystemStoragePath))
            {
                var fileSystemBinaryDataStorage = new FileSystemBinaryDataStorage(fileSystemStoragePath);
                var fileSystemStorage = new BlobRdDataStorage(
                    DataStorageTypes.FileSystem,
                    fileSystemBinaryDataStorage,
                    rdDataStorages.Single(x => x.Id == DataStorageTypes.MongoDB),
                    idGeneratorManager);
                rdDataStorages.Add(fileSystemStorage);
            }
        }

        private void SetupAzureBlobStorages(List<IRdDataStorage> rdDataStorages, IdGeneratorManager idGeneratorManager)
        {
            var azureBlobStorageAccountName = Configuration["AzureBlobStorage:User"];
            if (!string.IsNullOrWhiteSpace(azureBlobStorageAccountName))
            {
                var accessKey = Secrets.Get(Configuration["AzureBlobStorage:AccessKeyEnvironmentVariable"]);
                var azureCloudAccount = new CloudStorageAccount(
                    new StorageCredentials(azureBlobStorageAccountName, accessKey),
                    useHttps: true);
                var azureBlobStorage = new AzureBinaryDataStorage(azureCloudAccount);
                var rdAzureBlobStorage = new BlobRdDataStorage(
                    DataStorageTypes.AzureBlobStorage,
                    azureBlobStorage,
                    rdDataStorages.Single(x => x.Id == DataStorageTypes.MongoDB),
                    idGeneratorManager);
                rdDataStorages.Add(rdAzureBlobStorage);
            }
        }

        private void SetupSqlStorages(List<IRdDataStorage> rdDataStorages, IdGeneratorManager idGeneratorManager)
        {
            var sqlTableSetups = GetSqlTableSetupsFromConfiguration();
            if (sqlTableSetups.Any())
            {
                var existingSqlTablesStorage = new ExistingMssqlTablesRdDataStorage(
                    DataStorageTypes.ExistingSQL,
                    rdDataStorages.Single(x => x.Id == DataStorageTypes.MongoDB),
                    sqlTableSetups);
                rdDataStorages.Add(existingSqlTablesStorage);
            }

            var genericSqlDatabaseServer = Configuration["SQL:GenericDatabase:Server"];
            if (!string.IsNullOrWhiteSpace(genericSqlDatabaseServer))
            {
                var sqlRdDataStorage = new MssqlRdDataStorage(
                    DataStorageTypes.GenericSQL,
                    genericSqlDatabaseServer,
                    Configuration["SQL:GenericDatabase:DatabaseName"],
                    Configuration["SQL:GenericDatabase:Username"],
                    Secrets.Get(Configuration["SQL:GenericDatabase:PasswordEnvironmentVariable"]),
                    idGeneratorManager);
                rdDataStorages.Add(sqlRdDataStorage);
            }
        }

        private List<SqlTableSetup> GetSqlTableSetupsFromConfiguration()
        {
            var index = 0;
            var sqlTableSetups = new List<SqlTableSetup>();
            while (Configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.DataType)}"] != null)
            {
                var sqlTableSetup = new SqlTableSetup(
                    Configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.DataType)}"],
                    Configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.Server)}"],
                    Configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.TableName)}"],
                    Configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.Username)}"],
                    Configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.PasswordEnvironmentVariable)}"],
                    Configuration[$"SQL:ExistingTables:{index}:{nameof(SqlTableSetup.IdColumnName)}"]);
                sqlTableSetups.Add(sqlTableSetup);
                index++;
            }

            return sqlTableSetups;
        }
    }
}
