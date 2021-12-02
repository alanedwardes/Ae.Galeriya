using Ae.Galeriya.Piwigo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Galeriya.Console
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            services.AddSingleton<IConfiguration>(builder.Build());

            services.AddLettuceEncrypt();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMiddleware<PiwigoMiddleware>();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
