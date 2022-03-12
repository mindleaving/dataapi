using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DataAPI.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace DataAPI.Web.Setups
{
    public class OpenApiSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options => 
            { 
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Bearer token authentication",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new List<string>() }
                });
                options.SwaggerDoc($"v{ApiVersion.Current}", new OpenApiInfo
                {
                    Title = "DataAPI", 
                    Version = $"v{ApiVersion.Current}"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
        }
    }
}
