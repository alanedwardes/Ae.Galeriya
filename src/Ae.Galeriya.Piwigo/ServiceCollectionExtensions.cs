using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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

            return services.AddSingleton(configuration)
                .AddScoped<IPiwigoWebServiceMethodRepository, PiwigoWebServiceMethodRepository>()
                .AddSingleton<IPiwigoImageDerivativesGenerator, PiwigoImageDerivativesGenerator>()
                .AddSingleton<IPiwigoPhotosPageGenerator, PiwigoPhotosPageGenerator>()
                .AddSingleton<IUploadRepository, UploadRepository>();
        }
    }
}
