
using Connector.Connections;
using ESR.Hosting;
using ESR.Hosting.Client.TokenStorage;
using ESR.Hosting.Client.TokenStorage.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;


namespace Connector.Client
{
    public class CustomeAuthHandler2 : DelegatingHandler
    {
        private readonly CustomAuth _customAuth;

        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

        private readonly ITokenStorageClient _tokenStorageClient;

        private readonly ILogger<CustomeAuthHandler2> _logger;

        private readonly int _appId;

        private readonly int _appRegistrationId;

        private bool _canReAuthUnauthorized = true;

        private string _token;

        private string _refreshToken;

        private string _lockKey;

        private string _credentialHash;

        public CustomeAuthHandler2(CustomAuth customAuth)

        {
            _customAuth = customAuth;

            //_systemConfig = systemConfig ?? throw new ArgumentNullException(nameof(systemConfig));

            //_tokenStorageClient = tokenStorageClient ?? throw new ArgumentNullException(nameof(tokenStorageClient));

            //_appId = hostContext?.GetSystem()?.Id ?? throw new ArgumentNullException(nameof(hostContext));

            //_appRegistrationId = hostContext?.GetSystemConfig()?.Id ?? throw new ArgumentNullException(nameof(hostContext));

            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)

        {

            await InitializeToken(request, cancellationToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)

            {

                await InitializeToken(request, cancellationToken, true);

                return await base.SendAsync(request, cancellationToken);

            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && _canReAuthUnauthorized)

            {

                await InitializeToken(request, cancellationToken, true);

                _canReAuthUnauthorized = false;

                return await base.SendAsync(request, cancellationToken);

            }

            return response;

        }

        private async System.Threading.Tasks.Task InitializeToken(HttpRequestMessage request, CancellationToken cancellationToken, bool forceRefresh = false)

        {
            if (_token == null || forceRefresh)
            {
                var tokenResponse = await base.SendAsync(GetRefreshTokenRequest(), cancellationToken);
                tokenResponse.EnsureSuccessStatusCode();
                var resp = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);

                var token = JsonSerializer.Deserialize<TokenResponse>(resp, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _token = token.AccessToken;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        }


        private async System.Threading.Tasks.Task GetSavedToken()

        {

            var hash = await GetCredentialHash();

            var response = await _tokenStorageClient.GetToken(_appId, hash);

            if (response != null)

            {

                SetInternalToken(response);

            }

        }

        private void SetInternalToken(JsonDocument tokenResponse)
        {

            if (!tokenResponse.RootElement.TryGetNestedProperty("access_token", out var accessToken))

            {

                _logger.LogError($"OAuth Token Response did not contain an 'access_token' property. Token Response Content: {tokenResponse.RootElement.ToString()}");

                throw new Exception("OAuth Token Response did not contain an 'access_token' property.");

            }

            _token = accessToken.ToString();

            if (tokenResponse.RootElement.TryGetNestedProperty("refresh_token", out var refreshToken))

            {

                _refreshToken = refreshToken.ToString();

            }

        }

        private HttpRequestMessage GetRefreshTokenRequest()
        {
            var tokenUrl = GetRefreshTokenUrl();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {

                Content = new FormUrlEncodedContent(
                    new[]{
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                        ,new KeyValuePair<string, string>("client_id", _customAuth?.ClientID )
                        ,new KeyValuePair<string, string>("client_secret", _customAuth?.ClientSecret )
                    })
            };
            return tokenRequest;

        }

        private string GetRefreshTokenUrl()
        {
            return _customAuth.TokenUrl;
        }

        private async System.Threading.Tasks.Task<string> GetCredentialHash()
        {
            if (_credentialHash != null) return _credentialHash;

            var clientId = _customAuth?.ClientID;
            var clientSecret = _customAuth?.ClientSecret;
            _credentialHash = $"client_credentials::{GetRefreshTokenUrl()}::{clientId}::{clientSecret}".ToMd5();

            return _credentialHash;

        }

    }

}
