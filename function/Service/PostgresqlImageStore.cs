using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class PostgresqlImageStore : IImageStore
    {
        private readonly IConfigurationService _configService;

        public PostgresqlImageStore(IConfigurationService configService)
        {
            _configService = configService;

        }
        public async Task<IEnumerable<TrashType>> GetTrashTypes()
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
                {
                    await conn.OpenAsync();
                    string query = "SELECT * FROM campaign.\"trash_type\"";


                    var result = await conn.QueryAsync<TrashType>(query);

                    return result;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<bool> AnnotateImage(ImageAnnotationBoundingBoxResult aBbox)
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
                {
                    await conn.OpenAsync();
                    Guid id = Guid.NewGuid();
                    var insertQuery = "INSERT INTO label.\"bounding_boxes\" (id, id_creator_fk, createdon, id_ref_trash_type_fk, id_ref_images_for_labelling, location_x, location_y, width, height) VALUES ( @Id, @CreatorId, current_timestamp, @TrashId, @ImageId, @Location_x, @Location_y, @Width, @Height)";
     
                    var result = await conn.ExecuteAsync(insertQuery,
                        new
                        {
                            Id = id,
                            CreatorId = aBbox.CreatorId,
                            TrashId =aBbox.TrashId,
                            ImageId = aBbox.ImageId,
                            Location_x = aBbox.Location_x,
                            Location_y = aBbox.Location_y,
                            Width= aBbox.Width,
                            Height= aBbox.Height
                        }
                    ) ;

                    return result > 0 ;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<bool> InsertImageData(ImageLabel img)
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
                {
                    await conn.OpenAsync();
                    var query = $"INSERT INTO label.\"images_for_labelling\" (id, id_creator_fk, createdon, filename, view, image_quality, context, container_url) VALUES (@Id, (SELECT id from campaign.\"user\" WHERE id=@UserId), @CreatedOn, @Filename, @View, @ImgQuality, @Context, @Url)";

                    var result = await conn.ExecuteAsync(query,
                        new
                        {
                            Id= img.Id,
                            UserId= img.Id_creator_fk,
                            View = img.View,
                            Filename= img.FileName,
                            ImgQuality = img.Image_Quality,
                            Context = img.Context,
                            Url= img.Container_url,
                            @CreatedOn= img.Createdon

                        }
                    );

                    return result > 0;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<bool> UpdateImageData(ImageLabelViewModel img)
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
                {
                    await conn.OpenAsync();
                    var query = "UPDATE  label.\"images_for_labelling\" SET view = '@View', image_quality = '@ImgQuality', context = '@Context' WHERE id = '@imageId'";

                    var result = await conn.ExecuteAsync(query,
                        new
                        {
                            View = img.View,
                            ImgQuality = img.ImgQuality,
                            Context = img.Context,
                            ImageId = img.ImageId,
                           
                        }
                    );

                    return result > 0;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<IEnumerable<ImageAnnotationBoundingBox>> GetBBoxForOneImage(Guid id)
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
                {
                    await conn.OpenAsync();
                    string query = $"SELECT * FROM label.\"bounding_boxes\" WHERE label.\"bounding_boxes\".id_ref_images_for_labelling = \'{id}\'";


                    var result = await conn.QueryAsync<ImageAnnotationBoundingBox>(query);

                    return result;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ImageLabel> GetARandomImage()
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(_configService.GetValue(ConfigurationServiceWellKnownKeys.PostgresqlDbConnectionString)))
                {
                    string query = "SELECT * FROM label.\"images_for_labelling\" WHERE view = '' ORDER BY random() LIMIT 1";
                    string query2 = "SELECT * FROMlabel.\"images_for_labelling\" ORDER BY random() LIMIT 1";
                    
                    await conn.OpenAsync();
                    var result = await conn.QueryAsync<ImageLabel>(query);
                    if(!result.Any())
                        result = await conn.QueryAsync<ImageLabel>(query2);
                    return result.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }


    }
}
