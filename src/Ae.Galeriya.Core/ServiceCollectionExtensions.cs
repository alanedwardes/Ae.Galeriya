﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ae.Galeriya.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGalleriaStore(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            return services.AddDbContext<GalleriaDbContext>(optionsAction)
                .AddTransient<IMediaInfoExtractor, MediaInfoExtractor>();
        }
    }
}
