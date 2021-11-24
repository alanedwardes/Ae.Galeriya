using Ae.Galeriya.Core;
using Ae.Galeriya.Piwigo;
using Amazon;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ae.Galeriya.Console
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMemoryCache(x => x.SizeLimit = 512_000_000);
            services.AddSingleton<IBlobRepository>(x =>
            {
                //var localBlobCache = new FileBlobRepository(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "galeriya2")));
                var localBlobCache = new MemoryCacheBlobRepository(x.GetRequiredService<IMemoryCache>());
                var remoteBlobRepository = new AmazonS3BlobRepository(new TransferUtility(RegionEndpoint.EUWest2), "ae-piwigo-test");
                return new CachingBlobRepository(localBlobCache, remoteBlobRepository);
            });
            services.AddPiwigo(new PiwigoConfiguration
            {
                BaseAddress = new Uri("http://192.168.178.21:5000")
            });

            services.AddCommonServices();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMiddleware<PiwigoMiddleware>();
        }
    }
}
