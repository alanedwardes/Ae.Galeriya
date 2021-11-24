using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xabe.FFmpeg.Downloader;

namespace Ae.Galeriya.Console
{

    public static class Program
    {
        [Verb("serve", HelpText = "Serve the Piwigo endpoints")]
        class ServeOptions
        {
        }

        [Verb("getffmpeg", HelpText = "Download FFMPEG")]
        class GetFfmpegOptions
        {
        }

        [Verb("adduser", HelpText = "Add a user to the database")]
        class AddUserOptions
        {
            [Option('u', "username", Required = true, HelpText = "The username for the new user")]
            public string Username { get; set; }
            [Option('p', "password", Required = true, HelpText = "The password for the new user")]
            public string Password { get; set; }
        }

        [Verb("deleteuser", HelpText = "Remove a user from the database")]
        class DeleteUserOptions
        {
            [Option('u', "username", Required = true, HelpText = "The username of the user to delete")]
            public string Username { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ServeOptions, GetFfmpegOptions, AddUserOptions, DeleteUserOptions>(args)
                          .WithParsed<ServeOptions>(options =>
                          {
                              var builder = Host.CreateDefaultBuilder()
                                  .ConfigureWebHostDefaults(webHostBuilder =>
                                  {
                                      webHostBuilder.ConfigureLogging(configureLogging => configureLogging.AddCommonLogging());
                                      webHostBuilder.UseUrls("http://0.0.0.0:5000");
                                      webHostBuilder.UseStartup<Startup>();
                                  });

                              builder.Build().Run();
                          })
                        .WithParsed<GetFfmpegOptions>(options =>
                        {
                            FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official).GetAwaiter().GetResult();
                        })
                        .WithParsed<AddUserOptions>(options =>
                        {
                            var manager = GetUserManager();

                            var result = manager.CreateAsync(new IdentityUser{UserName = options.Username}, options.Password)
                                                .GetAwaiter()
                                                .GetResult();

                            System.Console.WriteLine(result.ToString());
                        })
                        .WithParsed<DeleteUserOptions>(options => 
                        {
                            var manager = GetUserManager();

                            var user = manager.FindByNameAsync(options.Username).GetAwaiter().GetResult();
                            if (user == null)
                            {
                                System.Console.WriteLine($"User {options.Username} not found.");
                                return;
                            }

                            var result = manager.DeleteAsync(user).GetAwaiter().GetResult();
                            System.Console.WriteLine(result.ToString());
                        })
                        .WithNotParsed(errors => { });
        }

        private static UserManager<IdentityUser> GetUserManager()
        {
            var services = new ServiceCollection();
            services.AddCommonServices();
            services.AddLogging(configureLogging => configureLogging.AddCommonLogging());
            return services.BuildServiceProvider().GetRequiredService<UserManager<IdentityUser>>();
        }
    }
}
