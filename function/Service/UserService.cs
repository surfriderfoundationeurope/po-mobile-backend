using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

[assembly: InternalsVisibleTo("Surfrider.PlasticOrigins.Backend.Mobile.Tests")]
namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IUserService
    {
        Task<User> Register(string lastName, string firstName, int birthYear, string email, string password);
        Task<User> GetUserFromId(string registeredUserId);
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
