using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Jose;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Surfrider.PlasticOrigins.Backend.Mobile.Tests")]
namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IUserService
    {
        Task<User> Register(string lastName, string firstName, string birthYear, string email, string password);
        Task<User> GetUserFromId(string registeredUserId);
        Task<bool> CheckUserCredentials(string email, string password);
        Task<string> GenerateTokenFromPassword(string email, string password);
        Task<string> RefreshToken(JwtTokenContent token);
        Task<bool> UpdatePassword(JwtTokenContent token, string password);
    }

    internal class UserService : IUserService
    {
        private readonly IUserStore _userStore;
        private readonly int _defaultTokenValidityPeriod;
        private readonly string _jwtTokenKey;

        public UserService(IUserStore userStore, IConfigurationService configurationService)
        {
            _userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            _defaultTokenValidityPeriod = 60 * 48;
            _jwtTokenKey = configurationService.GetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey);
        }

        public async Task<User> Register(string lastName, string firstName, string birthYear, string email,
            string password)
        {
            // Check parameters
            //Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(lastName));
            //Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(firstName));
            //Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(password));
            //Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(email));
            //Contract.Requires<ArgumentOutOfRangeException>(birthYear < DateTime.Now.Year - 1, "BirthYear should be less than current year");

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Save to store
            string userId = await _userStore.Create(new User(null, lastName, firstName, birthYear, passwordHash, email));
            User user = new User(userId, lastName, firstName, birthYear, passwordHash, email);

            DateTime tokenValidityDate = DateTime.UtcNow.AddMinutes(_defaultTokenValidityPeriod);
            string token = GenerateUserToken(email, tokenValidityDate, userId);

            user.AuthToken = token;
            user.AuthTokenExpiration = tokenValidityDate;

            //Contract.Ensures(user != null);
            return user;
        }

        public async Task<User> GetUserFromId(string registeredUserId)
        {
            var rawUser = await _userStore.GetFromId(registeredUserId);
            return rawUser;
        }

        public async Task<bool> CheckUserCredentials(string email, string password)
        {
            try
            {
                var userHash = await _userStore.GetUserPasswordHash(email);
                return BCrypt.Net.BCrypt.Verify(password, userHash.Item2);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GenerateTokenFromPassword(string email, string password)
        {
            string userId;
            string passwordHash;
            try
            {
                (userId, passwordHash) = await _userStore.GetUserPasswordHash(email);
            }
            catch (Exception)
            {
                return null;
            }

            if (BCrypt.Net.BCrypt.Verify(password, passwordHash) == false)
                return null;

            var token = GenerateUserToken(email, DateTime.UtcNow.AddMinutes(_defaultTokenValidityPeriod), userId);

            return token;
        }

        public async Task<string> RefreshToken(JwtTokenContent token)
        {
            var newToken = GenerateUserToken(token.Email, DateTime.UtcNow.AddMinutes(_defaultTokenValidityPeriod), token.UserId);
            return newToken;
        }

        public async Task<bool> UpdatePassword(JwtTokenContent token, string password)
        {

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            try
            {
                await _userStore.UpdatePassword(token.UserId, passwordHash);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private string GenerateUserToken(string email, DateTime validityDate, string userId)
        {
            return InternalGenerateUserToken(email, validityDate, userId, _jwtTokenKey); ;
        }

        internal static string InternalGenerateUserToken(string email, DateTime validityDate, string userId, string signatureKey)
        {
            var payload = new JwtTokenContent()
            {
                Email = email,
                ExpiresAt = validityDate,
                UserId = userId
            };

            var privateKey = Encoding.UTF8.GetBytes(signatureKey);
            string token = Jose.JWT.Encode(payload, privateKey, JwsAlgorithm.HS512);

            return token;
        }
    }

    public class JwtTokenContent
    {
        [JsonProperty("sub")]
        public string Email { get; set; }
        [JsonProperty("uid")]
        public string UserId { get; set; }

        [JsonIgnore]
        public DateTime ExpiresAt
        {
            get => Utilities.ToDateTime(EpochExpiration, DateTimeKind.Utc);
            set => EpochExpiration = Utilities.ToEpochTime(value);
        }

        [JsonProperty("exp")]
        public long EpochExpiration { get; set; }

    }
}
