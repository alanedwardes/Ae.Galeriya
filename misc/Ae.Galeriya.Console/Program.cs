using Ae.Galeriya.Piwigo;
using System.Threading;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Ae.Piwigo.Console
{
    public static class Program
    {
        public static void Main()
        {
            FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official).GetAwaiter().GetResult();

            var server = new PiwigoHttpServer();

            server.Listen(CancellationToken.None).GetAwaiter().GetResult();

            System.Console.WriteLine("wibble");
            System.Console.ReadLine();
        }
    }
}
