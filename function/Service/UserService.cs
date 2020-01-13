using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Jose;

[assembly: InternalsVisibleTo("Surfrider.PlasticOrigins.Backend.Mobile.Tests")]
namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IUserService
    {
        Task<User> Register(string lastName, string firstName, int birthYear, string email, string password);
        Task<User> GetUserFromId(string registeredUserId);
        Task<bool> CheckUserCredentials(string email, string password);
        Task<string> GenerateTokenFromPassword(string email, string password, DateTime validityDate);
    }

    internal class UserService : IUserService
    {
        private readonly IUserStore _userStore;

        public UserService(IUserStore userStore)
        {
            if(userStore == null)
                throw new ArgumentNullException(nameof(userStore));

            _userStore = userStore;
        }

        public async Task<User> Register(string lastName, string firstName, int birthYear, string email,
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
            var userId = await _userStore.Save(new { lastName, firstName, birthYear, email, passwordHash});

            var user = new User(userId, lastName, firstName, birthYear);
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
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<string> GenerateTokenFromPassword(string email, string password, DateTime validityDate)
        {
            string userId;
            string passwordHash;
            try
            {
                (userId, passwordHash) = await _userStore.GetUserPasswordHash(email);
            }
            catch (Exception e)
            {
                return null;
            }

            if (BCrypt.Net.BCrypt.Verify(password, passwordHash) == false)
                return null;

            var payload = new Dictionary<string, object>()
            {
                { "sub", email },
                { "exp", Utilities.DateToEpochDate(validityDate) },
                { "uid", userId }
            };

            var privateKey = Encoding.UTF8.GetBytes("24ba53e4c45253154dd5421decbb2e2242c31134b4dbc5bbedcc1d4bce1b3da3c33354e14c331b54c3cce24b23211de4acdbb15c5c3e3bbd3adebaea45142dc1c4caeaca54aae4ee4dc2e15cac51bb5cbadbd54c42e2b1abaed2dbd3dd3aaebc3abce4b2be42ebbd13b1b14e3c5bcda24432e33de1b44225d5232a4ca2a1da35c31e5e53db2d5bc413c4ced32e4354e51ccccebaa1cabcaa3bbaa53a3b55a1c53c5341c42e1522a2dd53e5d353c3442eecdedebd1bb125aa251e32d5b2d1e5cdad34a42d44a4dcb345d23aa1e4ba5cb51545ada5dcacca2d3ebcb1b2534a234e5da3db31eac215534155354a453d2bb42334cdad1cd51e43bb1edca25abe2ab2");
            string token = Jose.JWT.Encode(payload, privateKey, JwsAlgorithm.HS512);

            string json = Jose.JWT.Decode(token, privateKey);

            return token;
        }
    }
    
    public class User
    {
        public User(string id, string lastName, string firstName, int birthYear)
        {
            Id = id;
            LastName = lastName;
            FirstName = firstName;
            BirthYear = birthYear;
        }

        public string Id { get; }
        public string LastName { get; }
        public string FirstName { get; }
        public int BirthYear { get; }
    }
}
