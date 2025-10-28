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

    //protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //{
    //    await GetToken(request, cancellationToken);
    //    var response = await base.SendAsync(request, cancellationToken);

    //    //If the token that exists has expired, the Status Code will be 401: "Unauthorized"
    //    if (response.StatusCode == HttpStatusCode.Unauthorized)
    //    {
    //        //The method GetToken will either get or refresh the token in order to retry to send the request.
    //        //If the retry fails for a second time, the response will be returned as "Unauthorized".
    //        await GetToken(request, cancellationToken, true);
    //        return await base.SendAsync(request, cancellationToken);
    //    }

    //    return response;
    //}

    //private async Task GetToken(HttpRequestMessage request, CancellationToken cancellationToken, bool refresh = false)
    //{
    //    if (string.IsNullOrEmpty(_token) || refresh)
    //    {

    //        try

    //        {

    //            //Stores the current token to reference later and check if it was already refreshed by another thread.

    //            var previousToken = _token;

    //            // Uses a semaphore to prevent concurrent logins -- this makes it so only one thread can enter this block at a time

    //            await _tokenSemaphore.WaitAsync(cancellationToken);

    //            // If the previous and current token no longer match after entering this block then another thread has already logged in, and there is no need for this thread to continue the log in process

    //            if (previousToken != _token)

    //            {

    //                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

    //                return;

    //            }

    //            var response = await base.SendAsync(GetTokenRequest(), cancellationToken);

    //            if (response.IsSuccessStatusCode)

    //            {

    //                var tokenRespose = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);

    //                if (tokenRespose == null)

    //                {

    //                    _logger.LogError("Error in JSON response while retrieving OAuth Token");

    //                    throw new JsonException("Error deserializing authentication JSON response while retrieving OAuth Token");

    //                }

    //                if (!tokenRespose.RootElement.TryGetNestedProperty("access_token", out var accessToken))

    //                {

    //                    _logger.LogError($"OAuth Token Response did not contain an 'access_token' property. Token Response Content: {tokenRespose.RootElement.ToString()}");

    //                    throw new InvalidOperationException("OAuth Token Response did not contain an 'access_token' property.");

    //                }

    //                _token = accessToken.ToString();

    //            }

    //            else

    //            {

    //                _token = null;

    //                _logger.LogError($"Unsuccessful response while retrieving OAuth Token. Please, verify your credentials. Status Code: {response.StatusCode}");

    //            }

    //        }

    //        finally

    //        {

    //            _tokenSemaphore.Release();

    //        }

    //    }

    //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

    //}

}