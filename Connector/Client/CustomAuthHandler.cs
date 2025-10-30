using Connector.Connections;
using ESR.Hosting;
using Newtonsoft.Json.Linq;
using Serilog.Sinks.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Connector.Client;

public class CustomAuthHandler : DelegatingHandler
{
    private readonly CustomAuth _customAuth;

    public CustomAuthHandler(CustomAuth customAuth)
    {
        _customAuth = customAuth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //request.Headers.Remove("X-Custom-Header");
        //request.Headers.Add("X-Custom-Header", _customAuth.CustomHeader);

        var tokenResponse = GetTokenRequest();

        return await base.SendAsync(request, cancellationToken);
    }

    public async Task GetTokenRequest()
    {
        //var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _customAuth.BaseUrl)
        //{


        //    Content = new FormUrlEncodedContent(new[]
        //    {
        //                new KeyValuePair<string, string>("username", _customAuth.ClientID),
        //                new KeyValuePair<string, string>("password", _customAuth.ClientSecrete),
        //                new KeyValuePair<string, string>("password", _customAuth.ClientCertBase64),
        //        }),
        //    Headers =
        //        {
        //            { "User-Agent", $"{_customAuth.UserAgent}" }
        //        }
        //};

        var tokenRequest = new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", _customAuth.ClientID},
                    {"client_secret",_customAuth.ClientSecret}
                };

        // Create form-encoded content
        var requestContent = new FormUrlEncodedContent(tokenRequest);

        // var requestContent = new FormUrlEncodedContent(tokenRequest);

        string url = "https://accounts.adp.com/auth/oauth/v2/token";

        HttpClient httpClient  = new HttpClient();
        var response = await httpClient.PostAsync(url, requestContent);
        var responseContent = await response.Content.ReadAsStringAsync();


    }

}