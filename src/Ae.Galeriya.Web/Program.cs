using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
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
    public class TestCookieManager : ICookieManager
    {
        private readonly ChunkingCookieManager _cookieManager;

        public TestCookieManager()
        {
            _cookieManager = new ChunkingCookieManager();
        }

        public string GetRequestCookie(HttpContext context, string key)
        {
            if (context.Request.Query.TryGetValue(key, out var queryValue))
            {
                return queryValue;
            }

            return _cookieManager.GetRequestCookie(context, key);
        }

        public void AppendResponseCookie(HttpContext context, string key, string value, CookieOptions options)
        {
            _cookieManager.AppendResponseCookie(context, key, value, options);
        }

        public void DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            _cookieManager.DeleteCookie(context, key, options);
        }
    }

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

            var uploadCache = new FileBlobRepository(new DirectoryInfo(configuration.UploadCacheDirectory));

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.ConfigureLogging(configureLogging => configureLogging.AddCommonLogging());
                    webHostBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddCommonServices(configuration);
                    services.AddSingleton(configuration);
                    services.AddSingleton<ITransferUtility, TransferUtility>();
                    services.AddSingleton<IBlobRepository>(x =>
                    {
                        var remoteBlobRepository = new AmazonS3BlobRepository(x.GetRequiredService<ITransferUtility>(), configuration.BucketName);
                        return new CachingBlobRepository(uploadCache, remoteBlobRepository);
                    });
                    services.AddPiwigo(new PiwigoConfiguration
                    {
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

            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, option =>
            {
                option.Cookie.Name = "pwg_id";
                option.ExpireTimeSpan = TimeSpan.FromDays(356 * 10);
                option.CookieManager = new TestCookieManager();
            });

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
