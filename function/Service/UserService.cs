using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Jose;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using System.Linq;

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
        string RefreshToken(JwtTokenContent token);
        Task<bool> UpdatePassword(JwtTokenContent token, string password);
        Task SetAccountConfirmed(string userId);
        Task ResetPassword(string email);
    }

    internal class UserService : IUserService
    {
        private readonly ILogger _log;

        private readonly IUserStore _userStore;
        private readonly int _defaultTokenValidityPeriod;
        private readonly string _jwtTokenKey;
        private readonly string _mailJetApiKey;
        private readonly string _mailjetApiSecret;
        private readonly string _mailjetSenderEmail;
        private readonly string _azureAcsConnectionString;
        private string _functionBaseUrl;

        // TODO This is a hack. We should have something like a "FilesystemService"
        public string BaseFunctionDirectory { get; set; }

        public UserService(IUserStore userStore, IConfigurationService configurationService, ILogger<UserService> log)
        {
            _log = log;
            _userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
            _defaultTokenValidityPeriod = 60 * 48;
            _jwtTokenKey = configurationService.GetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey);
            _mailJetApiKey = configurationService.GetValue(ConfigurationServiceWellKnownKeys.MailjetApiKey);
            _mailjetApiSecret = configurationService.GetValue(ConfigurationServiceWellKnownKeys.MailjetApiSecret);
            _functionBaseUrl = configurationService.GetValue(ConfigurationServiceWellKnownKeys.BaseFunctionUrl);
            _mailjetSenderEmail = configurationService.GetValue(ConfigurationServiceWellKnownKeys.MailjetSenderEmail);
            _azureAcsConnectionString = configurationService.GetValue(ConfigurationServiceWellKnownKeys.AzureAcsConnectionString);
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

            await SendEmail(email, originalEmailHtml, string.Empty, "Validation de ton compte Plastic Origins");

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

        public string RefreshToken(JwtTokenContent token)
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
            catch (Exception)
            {
                return false;
            }
        }

        public async Task SetAccountConfirmed(string userId)
        {
            await _userStore.SetAccountValidated(userId);
        }

        public async Task ResetPassword(string email)
        {
            //var user = await _userStore.GetFromId("06566a9d-e0f4-414d-849a-16f11888fc42");
            var user = await _userStore.GetFromEmail(email);
            if (user == null)
            {
                _log.LogInformation("#auth #reset User Not Found");
                return;
            }

            var newPassword = GetRandomPassword();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userStore.UpdatePassword(user.Id, passwordHash);
            string originalEmailHtml = await File.ReadAllTextAsync(Path.Combine(BaseFunctionDirectory, "../Assets/mail-newpassword.html"));
            originalEmailHtml = originalEmailHtml.Replace("%%PASSWORD%%", newPassword);

            await SendEmail(email, originalEmailHtml, string.Empty, "Reinitialisation de mot de passe");

        }

        private string GetRandomPassword()
        {
            List<string> words = new List<string>() {"Wax", "Planche", "Leash", "Soleil", "Poisson", "Plastique", "Ocean", "Poncho", "Canette", "Reef" };
            Random r = new Random();

            return $"{words[r.Next(0, words.Count -1)]}{words[r.Next(0, words.Count -1)]}{r.Next(100,999)}";
        }


        private string GenerateUserToken(string email, DateTime validityDate, string userId, bool isValidationEmail = false)
        {
            return InternalGenerateUserToken(email, validityDate, userId, _jwtTokenKey, isValidationEmail); ;
        }


        private async Task SendEmail(string to, string content, string textContent, string subject)
        {
            _log.LogInformation($"Sending email: {subject}. Destination: {to?.Substring(0, 3)}");
            // If Azure ACS is configured, use this instead of Mailjet
            if(!string.IsNullOrEmpty(_azureAcsConnectionString))
            {
                _log.LogInformation(" - Sending with Azure ACS");
                await SendEmailAzureAcs(to, content, textContent, subject);
                return;
            }

            if (string.IsNullOrWhiteSpace(_mailJetApiKey) || string.IsNullOrWhiteSpace(_mailjetApiSecret))
            {
                Console.WriteLine("No email configured, skipping email sending.");
                return;
            }

            _log.LogInformation(" - Sending with Mailjet");
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
                                {"Email", _mailjetSenderEmail},
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
                            subject
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

        private async Task SendEmailAzureAcs(string to, string content, string textContent, string subject)
        {
            EmailClient emailClient = new EmailClient(_azureAcsConnectionString);
            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    Azure.WaitUntil.Started,
                    _mailjetSenderEmail,
                    to,
                    subject,
                    content);

            _log.LogInformation(" - Mail Sent. OperationID: {}", emailSendOperation.Id);
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
