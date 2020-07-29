using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class InMemoryUserStore : IUserStore
    {
        private readonly Dictionary<string, User> _itemsDictionary = new Dictionary<string, User>();

        public Task<string> Create(User userData)
        {
            string id = Guid.NewGuid().ToString("N");
            var user = new User(id, null, null, null, null, null);
            user.Merge(userData.AllValues);

            _itemsDictionary.Add(id, user);
            return Task.FromResult(id);
        }

        public Task<User> GetFromId(string userId)
        {
            if (_itemsDictionary.ContainsKey(userId))
            {
                var item = _itemsDictionary[userId];
                return Task.FromResult(item);
            }

            return null;
        }

        public Task<bool> CheckAvailability()
        {
            return Task.FromResult(true);
        }

        public Task<Tuple<string, string>> GetUserPasswordHash(string userEmail)
        {
            var user = _itemsDictionary.First(u => u.Value.Email == userEmail);
            return Task.FromResult(new Tuple<string, string>(user.Key, user.Value.PasswordHash));
        }

        public Task<bool> UpdatePassword(string userId, string passwordHash)
        {
            var dbUser = _itemsDictionary.First(u => u.Key == userId);

            var updateUser = new User(null, null, null, null, passwordHash, null);
            dbUser.Value.Merge(updateUser.AllValues);

            return Task.FromResult(true);
        }
    }
}