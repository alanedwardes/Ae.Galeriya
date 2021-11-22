using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;

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
                .AddScoped<IPiwigoWebServiceMethodRepository, PiwigoWebServiceMethodRepository>()
                .AddSingleton<IPiwigoImageDerivativesGenerator, PiwigoImageDerivativesGenerator>()
                .AddSingleton<IPiwigoPhotosPageGenerator, PiwigoPhotosPageGenerator>()
                .AddSingleton<IUploadRepository, UploadRepository>();
        }
    }
}
