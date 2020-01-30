using System;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure
{
    public class TestUserToken
    {
        public string UserId { get; protected set; }
        public string Email { get; protected set; }
        public string Token { get; protected set; }
        public DateTime ExpiresAt { get; protected set; }
        
        public static TestUserToken Create(string userId, string email, DateTime expiresAt,
            string keySignatureKey)
        {
            return new TestUserToken()
            {
                Email = email,
                UserId = userId,
                Token = UserService.InternalGenerateUserToken(email, expiresAt, userId, keySignatureKey),
                ExpiresAt = expiresAt
            };
        }

        public AccessTokenResult ToAccessTokenResult()
        {
            return AccessTokenResult.Success(new User(UserId, "test", "test", "1980", "qskdljMOIJOifqsdf", "user@domain.tld"));
        }
    }
}