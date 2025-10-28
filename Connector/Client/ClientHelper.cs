using Connector.Client;
using Connector.Connections;
using ESR.Hosting;
using ESR.Hosting.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using Xchange.Connector.SDK.Client.AuthTypes;
using Xchange.Connector.SDK.Client.ConnectivityApi.Models;

namespace Connector.Client
{
    public static class ClientHelper
    {
        public static class AuthTypeKeyEnums
        {
            public const string CustomAuth = "customAuth";
        }

        public static void ResolveServices(this IServiceCollection serviceCollection, ConnectionContainer activeConnection)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            switch (activeConnection.DefinitionKey)
            {
                case AuthTypeKeyEnums.CustomAuth:
                    var configCustomAuth = JsonSerializer.Deserialize<CustomAuth>(activeConnection.Configuration, options);
                    serviceCollection.AddSingleton<CustomAuth>(configCustomAuth!);
                    serviceCollection.AddTransient<RetryPolicyHandler>();
                    serviceCollection.AddTransient<CustomAuthHandler>();
                    serviceCollection.AddTransient<CustomeAuthHandler2>();
                    serviceCollection.AddTransient<CertificateHandler>();
                    // Replace this line:
                    // .AddHttpMessageHandler<CertificateHandler>()

                    // With this line:                   
                    serviceCollection.AddHttpClient<ApiClient, ApiClient>(client => new ApiClient(client, configCustomAuth!.BaseUrl))
                        //.AddHttpMessageHandler<CustomAuthHandler>()
                        .ConfigurePrimaryHttpMessageHandler(sp =>
                                                    new CertificateHandler(configCustomAuth!)
                                                    {
                                                        AllowAutoRedirect = false
                                                    }
                                                )
                        .AddHttpMessageHandler<CustomeAuthHandler2>()
                        .AddHttpMessageHandler<RetryPolicyHandler>();
                    //serviceCollection.AddHttpClient<ApiClient, ApiClient>(client => new ApiClient(client, configCustomAuth!.BaseUrl)).AddHttpMessageHandler<CertificateHandler>().AddHttpMessageHandler<RetryPolicyHandler>();
                    break;
                default:
                    throw new Exception($"Unable to find services for definition key {activeConnection.DefinitionKey}");
            }
        }
    }
}