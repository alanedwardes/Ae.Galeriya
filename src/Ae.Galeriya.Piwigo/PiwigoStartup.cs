using Microsoft.AspNetCore.Builder;

namespace Ae.Galeriya.Piwigo
{
    public class PiwigoStartup
    {
        public void Configure(IApplicationBuilder app) => app.UseMiddleware<PiwigoMiddleware>();
    }
}
