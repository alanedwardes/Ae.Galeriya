using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo;
using Ae.Geocode.Google;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Xabe.FFmpeg;

namespace Ae.Galeriya.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new GaleriyaConfiguration();

            new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("config.json", true)
                .AddCommandLine(args)
                .Build()
                .Bind(configuration);

            FFmpeg.SetExecutablesPath(configuration.FfmpegDirectory);

            using (var commonServiceProvider = GetServiceProvider(configuration))
            using (var dbContext = commonServiceProvider.GetRequiredService<GaleriyaDbContext>())
            {
                dbContext.Database.EnsureCreated();
            }

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.ConfigureLogging(configureLogging => configureLogging.AddCommonLogging());
                    webHostBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IFileBlobRepository>(x =>
                    {
                        return new AutoCleaningFileBlobRepository(x.GetRequiredService<ILogger<AutoCleaningFileBlobRepository>>(), new DirectoryInfo(configuration.UploadCacheDirectory), configuration.UploadCacheDirectorySize);
                    });

                    services.AddCommonServices(configuration);
                    services.AddSingleton(configuration);
                    services.AddHttpClient<IGoogleGeocodeClient, GoogleGeocodeClient>(x => x.BaseAddress = new Uri("https://maps.googleapis.com/"))
                            .AddHttpMessageHandler(x => new GoogleGeocodeAuthenticationHandler(configuration.GoogleApiKey));
                    services.AddSingleton<ITransferUtility, TransferUtility>();
                    services.AddSingleton<IBlobRepository>(x =>
                    {
                        var remoteBlobRepository = new AmazonS3BlobRepository(x.GetRequiredService<ITransferUtility>(), configuration.BucketName);
                        return new CachingBlobRepository(x.GetRequiredService<IFileBlobRepository>(), remoteBlobRepository);
                    });

                    services.AddPiwigo(new PiwigoConfiguration
                    {
                        ChunkBlobRepository = x => x.GetRequiredService<IFileBlobRepository>(),
                        FileBlobRepository = x => x.GetRequiredService<IFileBlobRepository>()
                    });
                });

            builder.Build().Run();
        }

        private static ServiceProvider GetServiceProvider(GaleriyaConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddCommonServices(configuration);
            services.AddLogging(configureLogging => configureLogging.AddCommonLogging());
            return services.BuildServiceProvider();
        }

        public static IServiceCollection AddCommonServices(this IServiceCollection services, GaleriyaConfiguration configuration)
        {
            services.AddIdentity<User, Role>()
                    .AddDefaultTokenProviders()
                    .AddEntityFrameworkStores<GaleriyaDbContext>();

            services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(configuration.DataProtectionDirectory));

            return services.AddGaleriyaStore(x => x.UseMySql(configuration.ConnectionString, ServerVersion.AutoDetect(configuration.ConnectionString)));
        }

        public static ILoggingBuilder AddCommonLogging(this ILoggingBuilder builder)
        {
            return builder
                .AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning)
                .AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Warning)
                .AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.Warning)
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .AddConsole(x => x.IncludeScopes = true);
        }
    }
}
