﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSW.Consulting.Application.Common.Interfaces;
using System;

namespace SSW.Consulting.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Secrets Provider
#if DEBUG
            bool useLocalSecrets = Convert.ToBoolean(configuration["UseLocalSecrets"]);
            if (useLocalSecrets)
            {
                services.AddSingleton<ISecretsProvider, LocalSecretsProvider>();
            }
            else
#endif
            {
                services.AddSingleton<ISecretsProvider, KeyVaultSecretsProvider>();
                services.AddSingleton<IKeyVaultClientProvider, KeyVaultClientProvider>();
            }

            // ConfigureStorageProviders
            services.AddSingleton<ICloudBlobClientProvider, CloudBlobClientProvider>();
            services.AddSingleton<IStorageProvider, AzureStorageProvider>();

            services.AddScoped<IProfileStorageProvider, ProfileStorageProvider>();

            return services;
        }
    }
}
