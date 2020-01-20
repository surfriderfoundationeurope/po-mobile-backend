using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth;
using Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure;
using Xunit;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests
{
    public class RefreshTokenTest : SurfriderBaseFunctionTest
    {
        // Correct token should send a new token with renewed
        [Fact]
        public async void ValidToken_ShouldSendNewToken()
        {
            var user = TestUserToken.Create(base.TestUserId, base.TestUserEmail, base.TokenDuration,
                base.TokenSignatureKey);
            var request = TestFactory.CreateHttpRequest(user);
            var settings = new InMemoryConfigurationService();
            settings.SetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey, base.TokenSignatureKey);
            var userFunctionsHost = new UserFunctions(new UserService(new InMemoryUserStore(), settings), settings);

            var response = await userFunctionsHost.RunRefreshToken(request, user.ToAccessTokenResult(), Logger);
            
            Assert.NotNull(response as OkObjectResult);
            // TODO: Should return token
        }

        // Expired token should return unauthorized
        [Fact]
        public async void ExpiredToken_ShouldReturnException()
        {
            var user = TestUserToken.Create(base.TestUserId, base.TestUserEmail, base.TokenDuration.AddDays(-10),
                base.TokenSignatureKey);
            var request = TestFactory.CreateHttpRequest(user);
            var settings = new InMemoryConfigurationService();
            settings.SetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey, base.TokenSignatureKey);
            var userFunctionsHost = new UserFunctions(new UserService(new InMemoryUserStore(), settings), settings);

            var response = await userFunctionsHost.RunRefreshToken(request, AccessTokenResult.Expired(), Logger);

            Assert.NotNull(response as StatusCodeResult);
            Assert.Equal(401, (response as StatusCodeResult).StatusCode);
        }

        // No token should return unauthorized
        [Fact]
        public async void NoToken_ShouldReturnException()
        {
            var request = TestFactory.CreateHttpRequest();
            var settings = new InMemoryConfigurationService();
            settings.SetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey, base.TokenSignatureKey);
            var userFunctionsHost = new UserFunctions(new UserService(new InMemoryUserStore(), settings), settings);

            var response = await userFunctionsHost.RunRefreshToken(request, AccessTokenResult.NoToken(), Logger);

            Assert.NotNull(response as StatusCodeResult);
            Assert.Equal(401, (response as StatusCodeResult).StatusCode);
        }
    }
}
