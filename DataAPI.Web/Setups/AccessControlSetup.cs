using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Commons.Misc;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.DataStorage;
using DataAPI.Service.IdGeneration;
using DataAPI.Web.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DataAPI.Web.Setups
{
    public class AccessControlSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            SetupJwtTokenAuthentication(services, configuration);
            services.AddSingleton<AuthenticationModule>();
            services.AddSingleton<IIdPolicy, DefaultIdPolicy>();
            services.AddSingleton<AuthorizationModule>();
        }

        private void SetupJwtTokenAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtPrivateKeyEnvironmentVariable = configuration["Authentication:Jwt:PrivateKeyEnvironmentVariable"];
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
    }
}
