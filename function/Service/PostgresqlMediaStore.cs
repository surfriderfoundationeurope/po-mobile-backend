using System;
using System.Threading.Tasks;
using Dapper;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class PostgresqlMediaStore : IMediaStore
    {
        private readonly IConfigurationService _configService;

        public PostgresqlMediaStore(IConfigurationService configService)
        {
            _configService = configService;
        }

        public async Task AddMedia(Guid mediaId, string fileName, string userId, string campaignId, DateTime createdOn, Uri storageUri)
        {
            using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
            {
                await conn.OpenAsync();
                Guid id = Guid.NewGuid();
                string insertQuery = "INSERT INTO campaign.\"media\" (\"id\" ,filename ,createdby ,id_ref_campaign_fk ,isdeleted ,createdon ,blob_url) VALUES  ( @mediaId, @fileName, @userId,  (SELECT id from campaign.\"campaign\" WHERE id=@guidCampaignId) , B'0', @createdOn, @absoluteUri)";

                var absoluteUri = storageUri.AbsoluteUri;
                var guidCampaignId = new Guid(campaignId);
                var result = await conn.ExecuteAsync(insertQuery,
                    new
                    {
                        mediaId,
                        fileName,
                        userId,
                        guidCampaignId, 
                        createdOn,
                        absoluteUri
                    }
                );
            }
        }
    }

    public interface IMediaStore
    {
        Task AddMedia(Guid mediaId, string fileName, string userId, string campaignId, DateTime createdOn, Uri storageUri);
    }
}
