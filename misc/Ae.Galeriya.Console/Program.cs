using Ae.Galeriya.Piwigo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg.Downloader;

namespace Ae.Piwigo.Console
{
    public static class Program
    {
        public static void Main()
        {
            FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official).GetAwaiter().GetResult();

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.ConfigureLogging(configureLogging =>
                    {
                        configureLogging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                        configureLogging.AddConsole(x => x.IncludeScopes = true);
                    });
                    webHostBuilder.UseUrls("http://0.0.0.0:5000");
                    webHostBuilder.UseStartup<Startup>();
                });

            builder.Build().Run();
        }
    }
}
