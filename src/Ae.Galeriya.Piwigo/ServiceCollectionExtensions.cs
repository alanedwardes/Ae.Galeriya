using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using Ae.Galeriya.Piwigo.Entities;

namespace Ae.Galeriya.Piwigo
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPiwigo(this IServiceCollection services, IPiwigoConfiguration configuration)
        {
            foreach (var type in typeof(ServiceCollectionExtensions).Assembly.GetTypes().Where(x => x.IsClass && x.GetInterfaces().Contains(typeof(IPiwigoWebServiceMethod))))
            {
                services.AddScoped(typeof(IPiwigoWebServiceMethod), type);
            }

            return services.AddScoped<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton(configuration)
                .AddSingleton((IGaleriyaConfiguration)configuration)
                .AddGalleriaStore(x => x.UseSqlite("Data Source=test.sqlite"))
                .AddScoped<IPiwigoWebServiceMethodRepository, PiwigoWebServiceMethodRepository>()
                .AddSingleton<IPiwigoImageDerivativesGenerator, PiwigoImageDerivativesGenerator>()
                .AddSingleton<IUploadRepository, UploadRepository>();
        }
    }
}
