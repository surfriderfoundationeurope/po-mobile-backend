using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Jose;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("Surfrider.PlasticOrigins.Backend.Mobile.Tests")]
namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IUserService
    {
        public string BaseFunctionDirectory { get; set; }
        Task<User> Register(string lastName, string firstName, string birthYear, string email, string password);
        Task<User> GetUserFromId(string registeredUserId);
        Task<bool> CheckUserCredentials(string email, string password);
        Task<string> GenerateTokenFromPassword(string email, string password);
        Task<string> RefreshToken(JwtTokenContent token);
        Task<bool> UpdatePassword(JwtTokenContent token, string password);
        Task SetAccountConfirmed(string userId);
    }

    internal class UserService : IUserService
    {
        private readonly IUserStore _userStore;
        private readonly int _defaultTokenValidityPeriod;
        private readonly string _jwtTokenKey;
        private readonly string _mailJetApiKey;
        private readonly string _mailjetApiSecret;
        private string _functionBaseUrl;

        // TODO This is a hack. We should have something like a "FilesystemService"
        public string BaseFunctionDirectory { get; set; }

        public UserService(IUserStore userStore, IConfigurationService configurationService)
        {
            _userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            _defaultTokenValidityPeriod = 60 * 48;
            _jwtTokenKey = configurationService.GetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey);
            _mailJetApiKey = configurationService.GetValue(ConfigurationServiceWellKnownKeys.MailjetApiKey);
            _mailjetApiSecret = configurationService.GetValue(ConfigurationServiceWellKnownKeys.MailjetApiSecret);
            _functionBaseUrl = configurationService.GetValue(ConfigurationServiceWellKnownKeys.BaseFunctionUrl);
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

            // Send email
            var emailValidationToken = GenerateUserToken(email, DateTime.UtcNow.AddHours(2), userId, true);
            string originalEmailHtml = await File.ReadAllTextAsync(Path.Combine(BaseFunctionDirectory, "../Assets/mail-validateaccount.html"));
            originalEmailHtml = originalEmailHtml.Replace("%%YES_LINK%%", $"{_functionBaseUrl}/validate/{emailValidationToken}");

            await SendEmail(email, originalEmailHtml, string.Empty);

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

        public async Task SetAccountConfirmed(string userId)
        {
            await _userStore.SetAccountValidated(userId);
        }

        private string GenerateUserToken(string email, DateTime validityDate, string userId, bool isValidationEmail = false)
        {
            return InternalGenerateUserToken(email, validityDate, userId, _jwtTokenKey, isValidationEmail); ;
        }


        private async Task SendEmail(string to, string content, string textContent)
        {
            MailjetClient client = new MailjetClient(_mailJetApiKey,_mailjetApiSecret)
            {
                Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
                {
                    Resource = Send.Resource,
                }
                .Property(Send.Messages, new JArray {
                    new JObject {
                        {
                            "From",
                            new JObject {
                                {"Email", "plasticorigins_api@surfrider.eu"},
                                {"Name", "Surfrider"}
                            }
                        }, {
                            "To",
                            new JArray {
                                new JObject {
                                    {
                                        "Email",
                                        to
                                    }
                                    // , {
                                    // "Name",
                                    // "Christopher"
                                    // }
                                }
                            }
                        }, {
                            "Subject",
                            "Validation de ton compte Plastic Origins"
                        }, {
                            "TextPart",
                            textContent
                        }, {
                            "HTMLPart",
                            content
                        }, {
                            "CustomID",
                            "POValidateEmail"
                        }
                    }
                });
            MailjetResponse response = await client.PostAsync(request);
            if(!response.IsSuccessStatusCode)
                throw new ApplicationException("Unable to send email");
        }

        internal static string InternalGenerateUserToken(string email, DateTime validityDate, string userId, string signatureKey, bool isValidationEmail)
        {
            var payload = new JwtTokenContent()
            {
                Email = email,
                ExpiresAt = validityDate,
                UserId = userId,
            };

            if (isValidationEmail)
            {
                payload.SpecialRights = "validate-email";
            }

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
        [JsonProperty("sr")]
        public string SpecialRights { get; set; }
    }
}
