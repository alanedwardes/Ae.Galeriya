using Ae.Galeriya.Piwigo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Ae.Galeriya.Console
{
    public sealed class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json", false);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
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
