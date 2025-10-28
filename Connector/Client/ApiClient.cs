using Connector.Connections;
using ESR.Hosting;
using ESR.Hosting.Client.TokenStorage.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Connector.Client;

/// <summary>
/// A client for interfacing with the API via the HTTP protocol.
/// </summary>
public class ApiClient : ITargetSystemApiClient
{
    private readonly HttpClient _httpClient;
    private string _token;
    private string _credentialHash;

    public ApiClient (HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new System.Uri(baseUrl);
    }

    // Example of a paginated response.
    public async Task<ApiResponse<PaginatedResponse<T>>> GetRecords<T>(string relativeUrl, int page, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"{relativeUrl}?page={page}", cancellationToken: cancellationToken).ConfigureAwait(false);
        return new ApiResponse<PaginatedResponse<T>>
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Data = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PaginatedResponse<T>>(cancellationToken: cancellationToken) : default,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken: cancellationToken)
        };
    }

    // Example of an incremental response.
    public async Task<ApiResponse<IEnumerable<T>>> GetRecordsSince<T>(string relativeUrl, string since, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"{relativeUrl}?since={since}", cancellationToken: cancellationToken).ConfigureAwait(false);
        return new ApiResponse<IEnumerable<T>>
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Data = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<IEnumerable<T>>(cancellationToken: cancellationToken) : default,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken: cancellationToken)
        };
    }

    public async Task<ApiResponse> GetNoContent(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetAsync("no-content", cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new ApiResponse
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            RawResult = await response.Content.ReadAsStreamAsync(cancellationToken: cancellationToken)
        };
    }

    public async Task<ApiResponse> TestConnection(CancellationToken cancellationToken = default)
    {
        // The purpose of this method is to validate that successful and authorized requests can be made to the API.
        // In this example, we are using the GET "oauth/me" endpoint.
        // Choose any endpoint that you consider suitable for testing the connection with the API.

        var fullurl = $"https://api.adp.com/hr/v2/workers";
        var url = $"hr/v2/workers";

        var response = await _httpClient
            .GetAsync(url, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new ApiResponse
        {
            IsSuccessful = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
        };
    }

    private HttpRequestMessage GetRefreshTokenRequest()
    {
        var tokenUrl = GetRefreshTokenUrl();

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(
                new[]{
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                        ,new KeyValuePair<string, string>("client_id", "7b179c69-1f23-4b7e-9e93-7be81e377dcb" )
                        ,new KeyValuePair<string, string>("client_secret", "ba2b69c6-0116-4122-b6b8-0e281f820672")
                })
        };

        return tokenRequest;
    }

    private string GetRefreshTokenUrl()
    {
        var baseUrl = "https://accounts.adp.com"; // $"/auth/oauth/v2/token";

        //var configuredTokenUrl = new Uri(_systemConfig?.PrimarySpecConfig?.OauthClientCredentialsConfig?.TokenUrl?.ToString() ?? "", UriKind.RelativeOrAbsolute);
        var configuredTokenUrl = new Uri($"/auth/oauth/v2/token");
        if (configuredTokenUrl.IsAbsoluteUri)
            return configuredTokenUrl.ToString();

        if (!string.IsNullOrWhiteSpace(configuredTokenUrl.ToString()))
            return baseUrl + configuredTokenUrl.ToString();

        var oasTokenUrl = new Uri(string.IsNullOrWhiteSpace("") ? "https://accounts.adp.com/auth/oauth/v2/token" : "", UriKind.RelativeOrAbsolute);
        if (oasTokenUrl.IsAbsoluteUri)
            return oasTokenUrl.ToString();

        if (!string.IsNullOrWhiteSpace(oasTokenUrl.ToString()))
            return baseUrl + configuredTokenUrl.ToString();

        throw new Exception("No OAuth Token URL could be determined.");
    }

    private async Task<string> GetCredentialHash()
    {
        if (_credentialHash != null) return _credentialHash;

        var clientId = "7b179c69-1f23-4b7e-9e93-7be81e377dcb";
        var clientSecret = "ba2b69c6-0116-4122-b6b8-0e281f820672";
        _credentialHash = $"client_credentials::{GetRefreshTokenUrl()}::{clientId}::{clientSecret}".ToMd5();

        return _credentialHash;
    }
}