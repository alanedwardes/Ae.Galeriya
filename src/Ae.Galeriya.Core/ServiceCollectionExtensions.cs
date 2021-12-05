using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ae.Galeriya.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGaleriyaStore(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            return services.AddDbContext<GaleriyaDbContext>(optionsAction)
                .AddTransient<ICategoryPermissionsRepository, CategoryPermissionsRepository>()
                .AddTransient<IMediaInfoExtractor, MediaInfoExtractor>()
                .AddSingleton<IThumbnailGenerator, ThumbnailGenerator>();
        }
    }
}
