using System;
using System.Threading.Tasks;

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
        Task SetAccountValidated(string userId);
        Task<User> GetFromEmail(string email);
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
