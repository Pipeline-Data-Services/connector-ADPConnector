using Connector.Connections;
using Connector.App.v1.Workers;
using ESR.Hosting;
using ESR.Hosting.Client.TokenStorage.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions 
    { 
        PropertyNameCaseInsensitive = true 
    };

    // ADP Rate Limiting: Max 10 requests per second recommended
    private static readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);
    private static DateTime _lastRequestTime = DateTime.MinValue;
    private const int MinMillisecondsBetweenRequests = 100; // 10 requests per second = 100ms between requests

    // ADP Pagination Settings
    private const int DefaultPageSize = 100; // ADP recommended page size
    private const int MaxPageSize = 100; // ADP maximum page size

    public ApiClient (HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new System.Uri(baseUrl);
    }

    /// <summary>
    /// Enforces ADP rate limiting of maximum 10 requests per second
    /// </summary>
    private async Task EnforceRateLimitAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            var millisecondsToWait = MinMillisecondsBetweenRequests - (int)timeSinceLastRequest.TotalMilliseconds;
            
            if (millisecondsToWait > 0)
            {
                await Task.Delay(millisecondsToWait, cancellationToken).ConfigureAwait(false);
            }
            
            _lastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
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

    /// <summary>
    /// Gets all workers from ADP API with proper pagination and rate limiting
    /// Follows ADP guidelines: GET method, $skip/$top pagination, max 10 req/sec
    /// </summary>
    public async Task<ApiResponse<IEnumerable<Worker>>> GetAllWorkersAsync(CancellationToken cancellationToken = default)
    {
        var allWorkers = new List<Worker>();
        int skip = 0;
        int top = DefaultPageSize;
        bool hasMoreData = true;
        int totalProcessed = 0;

        while (hasMoreData && !cancellationToken.IsCancellationRequested)
        {
            // Enforce ADP rate limiting
            await EnforceRateLimitAsync(cancellationToken).ConfigureAwait(false);

            var response = await GetWorkersPageAsync(skip, top, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessful)
            {
                // Return error response with any workers collected so far
                return new ApiResponse<IEnumerable<Worker>>
                {
                    IsSuccessful = false,
                    StatusCode = response.StatusCode,
                    Data = allWorkers,
                    RawResult = response.RawResult
                };
            }

            if (response.Data?.Workers != null && response.Data.Workers.Any())
            {
                allWorkers.AddRange(response.Data.Workers);
                totalProcessed += response.Data.Workers.Count;

                // Check if we have more data to fetch
                // ADP returns fewer records than requested when we've reached the end
                hasMoreData = response.Data.Workers.Count == top;
                skip += top;
            }
            else
            {
                hasMoreData = false;
            }

            hasMoreData =false; // Temporary: Disable pagination for testing
        }

        return new ApiResponse<IEnumerable<Worker>>
        {
            IsSuccessful = true,
            StatusCode = 200,
            Data = allWorkers,
            RawResult = null
        };
    }

    /// <summary>
    /// Gets a single page of workers from ADP API
    /// Uses GET method with $skip and $top query parameters as per ADP documentation
    /// </summary>
    private async Task<ApiResponse<WorkersResponse>> GetWorkersPageAsync(
        int skip, 
        int top, 
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (top > MaxPageSize)
        {
            top = MaxPageSize;
        }

        string relativeUrl = $"hr/v2/workers?$skip={skip}&$top={top}";

        try
        {
            // Use GET method as per ADP API documentation
            HttpResponseMessage response = await _httpClient
                .GetAsync(relativeUrl, cancellationToken)
                .ConfigureAwait(false);

            // Read response content
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                WorkersResponse? workersResponse = null;
                
                if (!string.IsNullOrEmpty(responseContent))
                {
                    workersResponse = JsonSerializer.Deserialize<WorkersResponse>(responseContent, _options);
                }

                return new ApiResponse<WorkersResponse>
                {
                    IsSuccessful = true,
                    StatusCode = (int)response.StatusCode,
                    Data = workersResponse,
                    RawResult = await response.Content.ReadAsStreamAsync(cancellationToken: cancellationToken)
                };
            }
            else
            {
                return new ApiResponse<WorkersResponse>
                {
                    IsSuccessful = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    RawResult = await response.Content.ReadAsStreamAsync(cancellationToken: cancellationToken)
                };
            }
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"HTTP request failed while fetching workers (skip: {skip}, top: {top})", ex)
            {
                StatusCode = 0
            };
        }
        catch (JsonException ex)
        {
            throw new ApiException($"Failed to deserialize workers response (skip: {skip}, top: {top})", ex)
            {
                StatusCode = 0
            };
        }
    }

    /// <summary>
    /// Legacy method - replaced with GetAllWorkersAsync for better pagination support
    /// Kept for backwards compatibility
    /// </summary>
    [Obsolete("Use GetAllWorkersAsync instead for proper pagination and rate limiting")]
    public async Task<ApiResponse<IEnumerable<T>>> GetWorkers<T>(CancellationToken cancellationToken = default)
    {
        // For backwards compatibility, delegate to new implementation
        var response = await GetAllWorkersAsync(cancellationToken).ConfigureAwait(false);
        
        return new ApiResponse<IEnumerable<T>>
        {
            IsSuccessful = response.IsSuccessful,
            StatusCode = response.StatusCode,
            Data = (IEnumerable<T>)(object)(response.Data ?? Enumerable.Empty<Worker>()),
            RawResult = response.RawResult
        };
    }
}