using Ae.Galeriya.Core;
using Amazon;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
                    x.AddMvc();
                    x.AddSingleton<IBlobRepository>(x =>
                    {
                        var localBlobCache = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "galeriya"));
                        var remoteBlobRepository = new AmazonS3BlobRepository(new TransferUtility(RegionEndpoint.EUWest2), "ae-piwigo-test");
                        return new CachingBlobRepository(localBlobCache, remoteBlobRepository);
                    });
                    x.AddPiwigo(new PiwigoConfiguration
                    {
                        BucketName = "ae-piwigo-test",
                        BaseAddress = new Uri("http://192.168.178.21:5000")
                    });
                });

            await builder.Build().RunAsync(token);
        }
    }
}
