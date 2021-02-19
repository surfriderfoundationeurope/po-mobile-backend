using Dapper;
using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using System.Linq;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class TraceService
    {

        private TraceStore _traceStore;

        public TraceService(TraceStore traceStore)
        {
            _traceStore = traceStore;
        }

        public async Task AddTrace(string userId, TraceViewModel trace)
        {
            // ID
            // locomotion = move
            // isaidriven = trackingMode (manual)
            // id_ref_user_fk
            // riverside = bank - virrer le bank de "leftBank"
            // remark = ???
            // createdon = date

            // TODO: Create Trace object
            Trace dbTrace = new Trace();
            dbTrace.Id = Guid.Parse(trace.id);
            dbTrace.Locomotion = trace.move;
            dbTrace.IsAiDriven = trace.trackingMode.ToLower() != "manual";
            dbTrace.UserId = Guid.Parse(userId);
            dbTrace.Riverside = trace.bank.ToLower().Replace("bank", "");
            dbTrace.CapturedOn = trace.date;
            dbTrace.Remark = trace.comment;

            // Save to DB
            await _traceStore.CreateCampaign(dbTrace);

            // Upload to blob storage

        }
        public async Task AddTrajectoryPoints(TraceViewModel trace)
        {
            List<Position> positions = trace.positions.Select(p => new Position(p)).ToList();
            positions.ForEach(p =>
            {
                p.Id = Guid.NewGuid();
                p.RefCampaign = Guid.Parse(trace.id);
                p.Createdon = DateTime.Now;
            });
            // Save to DB
            await _traceStore.CreateTrajectoryPoints(positions);

            // Upload to blob storage

        }
    }

    public class TraceStore
    {
        private readonly IConfigurationService _configService;

        public TraceStore(IConfigurationService configService)
        {
            _configService = configService;
        }

        public async Task CreateCampaign(Trace trace)
        {
            using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
            {
                await conn.OpenAsync();
                string insertQuery = "INSERT INTO campaign.\"campaign\" (id, locomotion, isaidriven, remark, id_ref_user_fk, riverside, createdon) VALUES (@Id, @Locomotion, @IsAiDriven, @Remark, @UserId, @Riverside, @CapturedOn)";

                var result = await conn.ExecuteAsync(insertQuery, trace);
            }
        }
        public async Task CreateTrajectoryPoints(IEnumerable<Position> positions)
        {
            using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
            {
                await conn.OpenAsync();
                string insertQuery = "INSERT INTO campaign.\"trajectory_point\" (id, id_ref_campaign_fk, \"time\", lat, lon, createdon) VALUES (@Id, @RefCampaign, @Time, @Lat, @Lon, @Createdon)";

                var result = await conn.ExecuteAsync(insertQuery, positions);
            }
        }
    }

}
