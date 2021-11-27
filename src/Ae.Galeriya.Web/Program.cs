using Ae.Galeriya.Core;
using Ae.Galeriya.Piwigo;
using Amazon.S3.Transfer;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Ae.Galeriya.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ServeOptions>(args)
                          .WithParsed(options => Serve(options))
                          .WithNotParsed(errors => { });
        }

        private static void Serve(ServeOptions options)
        {
            var commonServiceProvider = GetServiceProvider();

            using (var dbContext = commonServiceProvider.GetRequiredService<GaleriaDbContext>())
            {
                dbContext.Database.EnsureCreated();
            }

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.ConfigureLogging(configureLogging => configureLogging.AddCommonLogging());
                    webHostBuilder.UseUrls(options.BindAddress);
                    webHostBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddCommonServices();
                    services.AddMemoryCache(x => x.SizeLimit = options.MemoryCacheSize);
                    services.AddSingleton<ITransferUtility>(new TransferUtility(Amazon.RegionEndpoint.EUWest2));
                    services.AddSingleton<IBlobRepository>(x =>
                    {
                        var localBlobCache = new MemoryCacheBlobRepository(x.GetRequiredService<IMemoryCache>());
                        var remoteBlobRepository = new AmazonS3BlobRepository(x.GetRequiredService<ITransferUtility>(), options.BucketName);
                        return new CachingBlobRepository(localBlobCache, remoteBlobRepository);
                    });
                    services.AddPiwigo(new PiwigoConfiguration
                    {
                        BaseAddress = options.BaseAddress
                    });
                });

            builder.Build().Run();
        }

        private static IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddCommonServices();
            services.AddLogging(configureLogging => configureLogging.AddCommonLogging());
            return services.BuildServiceProvider();
        }
    }
}
