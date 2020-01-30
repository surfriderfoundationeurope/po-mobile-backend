using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Newtonsoft.Json;
//using StackExchange.Redis;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IUserStore
    {
        Task<string> Create(User userData);
        Task<User> GetFromId(string userId);
        Task<bool> CheckAvailability();
        Task<Tuple<string, string>> GetUserPasswordHash(string userEmail);
        Task<bool> UpdatePassword(string userId, string passwordHash);
    }

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

    //public class RedisUserStore : IUserStore
    //{
    //    private ConnectionMultiplexer _redis;
    //    private readonly IConfigurationService _configurationService;

    //    public RedisUserStore(IConfigurationService configurationService)
    //    {
    //        if (_configurationService == null)
    //            throw new ArgumentNullException(nameof(configurationService));

    //        _configurationService = configurationService;
    //    }

    //    public async Task Initialize()
    //    {
    //        _redis = ConnectionMultiplexer.Connect(_configurationService.GetValue(ConfigurationServiceWellKnownKeys.RedisHost));
    //    }

    //    public async Task<string> Create(User userData)
    //    {
    //        await EnsureRedisClientIsInitialized();
    //        string id = Guid.NewGuid().ToString("N");

    //        IDatabase db = _redis.GetDatabase();
    //        await db.StringSetAsync(id, JsonConvert.SerializeObject(userData));
    //        await db.StringSetAsync($"email:{userData.Email}", id);
    //        return id;
    //    }

    //    private async Task EnsureRedisClientIsInitialized()
    //    {
    //        if (_redis == null)
    //            await Initialize();
    //    }

    //    public async Task<User> GetFromId(string userId)
    //    {
    //        await EnsureRedisClientIsInitialized();
    //        IDatabase db = _redis.GetDatabase();
    //        var obj = await db.StringGetAsync(userId);
    //        return BaseEntity.FromJson<User>(obj);
    //    }

    //    public async Task<bool> CheckAvailability()
    //    {
    //        await EnsureRedisClientIsInitialized();
    //        return _redis.IsConnected;
    //    }

    //    public async Task<Tuple<string, string>> GetUserPasswordHash(string userEmail)
    //    {
    //        await EnsureRedisClientIsInitialized();
    //        IDatabase db = _redis.GetDatabase();
    //        var userId = db.StringGet($"email:{userEmail}");
    //        if (string.IsNullOrEmpty(userId))
    //            return null;

    //        var user = await GetFromId(userId);
    //        if (user == null)
    //            return null;

    //        return new Tuple<string, string>(user.Id, user.PasswordHash);

    //    }

    //    public async Task<bool> UpdatePassword(string userId, string passwordHash)
    //    {
    //        await EnsureRedisClientIsInitialized();
    //        throw new NotImplementedException();
    //    }
    //}
}
