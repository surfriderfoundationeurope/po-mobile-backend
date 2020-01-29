using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IUserStore
    {
        Task<string> Save(dynamic userData);
        Task<User> GetFromId(string userId);
        bool CheckAvailability();
        Task<Tuple<string,string>> GetUserPasswordHash(string userEmail);
        Task<bool> UpdatePassword(string userId, string passwordHash);
    }

    public class InMemoryUserStore : IUserStore
    {
        private readonly Dictionary<string, dynamic> _itemsDictionary = new Dictionary<string, dynamic>();

        public Task<string> Save(dynamic userData)
        {
            string id = Guid.NewGuid().ToString("N");
            _itemsDictionary.Add(id, userData);
            return Task.FromResult(id);
        }

        public Task<User> GetFromId(string userId)
        {
            if (_itemsDictionary.ContainsKey(userId))
            {
                var item= _itemsDictionary[userId];
                return Task.FromResult(new User(userId, item.lastName, item.firstName, item.birthYear));
            }
            else
            {
                return null;
            }
        }

        public bool CheckAvailability()
        {
            return true;
        }

        public Task<Tuple<string, string>> GetUserPasswordHash(string userEmail)
        {

            var user = _itemsDictionary.First(u => u.Value.email == userEmail);
            return Task.FromResult(new Tuple<string, string>(user.Key, user.Value.passwordHash));
        }

        public Task<bool> UpdatePassword(string userId, string passwordHash)
        {
            var user = _itemsDictionary.First(u => u.Key == userId);
            _itemsDictionary[userId] = new
            {
                lastName = user.Value.lastName,
                firstName = user.Value.firstName,
                birthYear = user.Value.birthYear,
                email = user.Value.email,
                passwordHash
            };
            return Task.FromResult(true);
        }
    }

}
