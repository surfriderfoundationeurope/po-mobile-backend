using System;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth
{
    public sealed class AccessTokenResult
    {
        public AccessTokenStatus Status { get; private set; }
        
        public User User { get; private set; }

        public static AccessTokenResult Success(User user)
        {
            return new AccessTokenResult
            {
                User = user,
                Status = AccessTokenStatus.Valid
            };
        }

        public static AccessTokenResult Expired()
        {
            return new AccessTokenResult
            {
                Status = AccessTokenStatus.Expired
            };
        }

        public static AccessTokenResult Error(Exception ex)
        {
            return new AccessTokenResult
            {
                Status = AccessTokenStatus.Error
            };
        }

        public static AccessTokenResult NoToken()
        {
            return new AccessTokenResult
            {
                Status = AccessTokenStatus.NoToken
            };
        }

    }
}
