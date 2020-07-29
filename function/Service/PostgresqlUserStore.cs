using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class PostgresqlUserStore : IUserStore
    {
        private readonly IConfigurationService _configService;

        public PostgresqlUserStore(IConfigurationService configService)
        {
            _configService = configService;
        }
        
        public async Task<string> Create(User userData)
        {
            using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
            {
                await conn.OpenAsync();
                Guid id = Guid.NewGuid();
                string insertQuery = "INSERT INTO campaign.\"user\" (id,firstname,lastname,email,emailconfirmed,passwordhash,yearofbirth,isdeleted,createdon) VALUES (@Id, @FirstName, @LastName, @Email, FALSE, @PassHash, @BirthYear, FALSE, @CreationTime)";

                var result = await conn.ExecuteAsync(insertQuery,
                    new
                    {
                        Id = id,
                        userData.FirstName,
                        userData.LastName,
                        userData.Email,
                        PassHash = userData.PasswordHash,
                        BirthYear = new DateTime(Convert.ToInt32(userData.BirthYear), 01, 01),
                        CreationTime = DateTime.UtcNow
                    }
                );

                return id.ToString("D");
            }
        }

        public async Task<User> GetFromId(string userId)
        {
            using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
            {
                await conn.OpenAsync();
                var result = await conn.QueryAsync<User>(
                    "select  id,lastname,firstname,yearofbirth BirthYear,email,passwordhash from campaign.\"user\" WHERE isdeleted = FALSE AND id = '@Id'", 
                    new { Id = userId }
                    );
                return result.FirstOrDefault();
            }
        }

        public async Task<bool> CheckAvailability()
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
                {
                    await conn.OpenAsync();
                    await conn.QueryAsync("SELECT version();");
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<Tuple<string, string>> GetUserPasswordHash(string userEmail)
        {
            using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
            {
                await conn.OpenAsync();
                var result = await conn.QueryFirstAsync(
                    "select id, passwordHash from campaign.\"user\" WHERE isdeleted = FALSE AND email = @Email",
                    new { Email = userEmail }
                );

                var row = result as IDictionary<string, object>;

                if (row == null)
                    return null;

                return new Tuple<string, string>(row["id"].ToString(), row["passwordhash"].ToString());
            }
        }

        public Task<bool> UpdatePassword(string userId, string passwordHash)
        {
            throw new NotImplementedException();
        }
    }
}
