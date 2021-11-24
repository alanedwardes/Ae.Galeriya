using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ae.Galeriya.Console
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<GaleriaDbContext>();

            return services.AddGalleriaStore(x => x.UseSqlite("Data Source=test.sqlite"));
        }

        public static ILoggingBuilder AddCommonLogging(this ILoggingBuilder builder)
        {
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            return builder.AddConsole(x => x.IncludeScopes = true);
        }
    }
}
