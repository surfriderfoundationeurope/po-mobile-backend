using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    interface IUserStore
    {
        Task<string> Save(dynamic userData);
        Task<User> GetFromId(string userId);
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
    }

}
