using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WeatherMVC.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IOptions<IdentityServerSettings> _identityServerSettings;
        private readonly IConfiguration _configuration;
        private readonly DiscoveryDocumentResponse _discoveryDocument;

        public TokenService(ILogger<TokenService> logger, IOptions<IdentityServerSettings> identityServerSettings, IConfiguration configuration)
        {
            _logger = logger;
            _identityServerSettings = identityServerSettings;
            _configuration = configuration;
            using var httpClient = new HttpClient();
            _discoveryDocument = httpClient.GetDiscoveryDocumentAsync(identityServerSettings.Value.DiscoveryUrl).Result;
            if (_discoveryDocument.IsError)
            {
                logger.LogError($"Unable to get discovery document. Error is: {_discoveryDocument.Error}");
                throw new Exception("Unable to get discovery document", _discoveryDocument.Exception);
            }
        }

        public async Task<TokenResponse> GetToken(string scope)
        {
            using var client = new HttpClient();
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = _discoveryDocument.TokenEndpoint,

                ClientId = _identityServerSettings.Value.ClientName,
                ClientSecret = _identityServerSettings.Value.ClientPassword,
                Scope = scope
            });


            if (tokenResponse.IsError)
            {
                _logger.LogError($"Unable to get token. Error is: {tokenResponse.Error}");
                throw new Exception("Unable to get token", tokenResponse.Exception);
            }

            return tokenResponse;
        }

        public async Task<TokenResponse> GetRefreshToken(string refreshToken)
        {
            using var client = new HttpClient();

            var tokenResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = _discoveryDocument.TokenEndpoint,
                ClientId = _configuration["InteractiveServiceSettings:ClientId"],
                ClientSecret = _configuration["InteractiveServiceSettings:ClientSecret"],
                RefreshToken = refreshToken
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError($"Unable to get token. Error is: {tokenResponse.Error}");
                throw new Exception("Unable to get token", tokenResponse.Exception);
            }

            return tokenResponse;
        }

    }
}