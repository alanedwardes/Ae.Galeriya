using Ae.Galeriya.Core;
using Ae.Galeriya.Piwigo;
using System.IO;
using System.Threading;
using Xabe.FFmpeg;

namespace Ae.Piwigo.Console
{
    public static class Program
    {
        public static void Main()
        {
            FFmpeg.SetExecutablesPath(@"C:\Users\alan\Downloads\ffmpeg-4.3.1-2020-11-19-full_build\bin");
            //var path1 = @"C:\Users\alan\Downloads\IMG_3135.JPG";
            //var path2 = @"C:\Users\alan\Downloads\IMG_3359.MOV";
            //var path3 = @"C:\Users\alan\Downloads\VID_20200519_120224.mp4";
            //var path4 = @"C:\Users\alan\Downloads\2021-11-17T21 11 36.mp4";

            //var test = new MediaInfoExtractor();

            //test.ExtractSnapshot(new FileInfo(path3), new FileInfo(@"C:\Users\alan\Desktop\New folder (11)\test.jpg"), CancellationToken.None).GetAwaiter().GetResult();

            //var test2 = test.ExtractInformation(new FileInfo(path4), CancellationToken.None).GetAwaiter().GetResult();

            var server = new PiwigoHttpServer();

            server.Listen(CancellationToken.None).GetAwaiter().GetResult();

            System.Console.WriteLine("wibble");
            System.Console.ReadLine();
        }
    }
}
