using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoHttpServer
    {
        public async Task Listen(CancellationToken token)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x =>
                {
                    x.ConfigureLogging(new Action<ILoggingBuilder>((y) =>
                    {
                        y.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                        y.AddConsole(x => x.IncludeScopes = true);
                    }));
                    x.UseUrls("http://0.0.0.0:5000");
                    x.UseStartup<PiwigoStartup>();
                })
                .ConfigureServices(x =>
                {
                    x.AddPiwigo();
                });

            await builder.Build().RunAsync(token);
        }
    }
}
