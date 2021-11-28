﻿using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                .AddCommandLine(args)
                .Build()
                .Bind(configuration);

            FFmpeg.SetExecutablesPath(configuration.FfmpegDirectory);

            using (var commonServiceProvider = GetServiceProvider(configuration))
            using (var dbContext = commonServiceProvider.GetRequiredService<GaleriyaDbContext>())
            {
                dbContext.Database.EnsureCreated();
            }

            var uploadCache = new FileBlobRepository(new DirectoryInfo(configuration.UploadCacheDirectory));

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.ConfigureLogging(configureLogging => configureLogging.AddCommonLogging());
                    webHostBuilder.UseUrls(configuration.BindAddress);
                    webHostBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddCommonServices(configuration);
                    services.AddSingleton(configuration);
                    services.AddMemoryCache(x => x.SizeLimit = configuration.MemoryCacheSize);
                    services.AddSingleton<ITransferUtility, TransferUtility>();
                    services.AddSingleton<IBlobRepository>(x =>
                    {
                        var localBlobCache = new MemoryCacheBlobRepository(x.GetRequiredService<IMemoryCache>());
                        var remoteBlobRepository = new AmazonS3BlobRepository(x.GetRequiredService<ITransferUtility>(), configuration.BucketName);
                        return new CachingBlobRepository(localBlobCache, remoteBlobRepository);
                    });
                    services.AddPiwigo(new PiwigoConfiguration
                    {
                        BaseAddress = configuration.BaseAddress,
                        ChunkBlobRepository = uploadCache,
                        FileBlobRepository = uploadCache
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

            var sqliteConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = configuration.SqliteDatabaseFile
            };

            return services.AddGaleriyaStore(x => x.UseSqlite(sqliteConnectionString.ConnectionString));
        }

        public static ILoggingBuilder AddCommonLogging(this ILoggingBuilder builder)
        {
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            return builder.AddConsole(x => x.IncludeScopes = true);
        }
    }
}
