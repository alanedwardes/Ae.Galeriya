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
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            services.AddLettuceEncrypt(x => config.Bind("LettuceEncrypt", x));
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMiddleware<PiwigoMiddleware>();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
