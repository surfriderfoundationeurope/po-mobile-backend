using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth
{
    public class AccessTokenValueProvider : IValueProvider
    {
        private const string AuthHeaderName = "Authorization";
        private const string BearerPrefix = "Bearer ";
        private readonly HttpRequest _request;
        private readonly IConfigurationService _configurationService;

        public Type Type => typeof(ClaimsPrincipal);

        public string ToInvokeString() => string.Empty;

        public AccessTokenValueProvider(HttpRequest request)
        {
            _request = request;
            // TODO: Find a better way
            _configurationService = new EnvironmentConfigurationService();
        }

        public async Task<object> GetValueAsync()
        {
            try
            {
                // Get the token from the header
                if (_request != null &&
                    _request.Headers.ContainsKey(AuthHeaderName) &&
                    _request.Headers[AuthHeaderName].ToString().StartsWith(BearerPrefix))
                {
                    var token = _request.Headers[AuthHeaderName].ToString().Substring(BearerPrefix.Length);
                    var decodedToken = await GetRawToken(_request, _configurationService.GetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey));

                    if (decodedToken == null)
                        return AccessTokenResult.NoToken();

                    return decodedToken.ExpiresAt < DateTime.UtcNow 
                        ? AccessTokenResult.Expired() 
                        : AccessTokenResult.Success(new User(decodedToken.UserId, null, null, null));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(AccessTokenResult.Error(ex));
            }

            return AccessTokenResult.NoToken();
        }

        public static async Task<JwtTokenContent> GetRawToken(HttpRequest req, string signatureKey)
        {
            // Get the token from the header
            if (
                req == null 
                || !req.Headers.ContainsKey(AuthHeaderName) 
                ||
                !req.Headers[AuthHeaderName].ToString().StartsWith(BearerPrefix)) 
                return null;
            
            var token = req.Headers[AuthHeaderName].ToString().Substring(BearerPrefix.Length);
            var privateKey = Encoding.UTF8.GetBytes(signatureKey);
            var decodedToken = Jose.JWT.Decode<JwtTokenContent>(token, privateKey);

            return decodedToken;

        }
    }
}