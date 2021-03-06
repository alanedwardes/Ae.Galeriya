using Ae.MediaMetadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ae.Galeriya.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGaleriyaStore(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            return services.AddDbContext<GaleriyaDbContext>(optionsAction, ServiceLifetime.Transient)
                .AddTransient<IPhotoCreator, PhotoCreator>()
                .AddSingleton<IPhotoMigrator, PhotoMigrator>()
                .AddTransient<ITagRepository, TagRepository>()
                .AddTransient<ICategoryPermissionsRepository, CategoryPermissionsRepository>()
                .AddTransient<IMediaInfoExtractor, MediaInfoExtractor>()
                .AddSingleton<IThumbnailGenerator, ThumbnailGenerator>();
        }
    }
}
