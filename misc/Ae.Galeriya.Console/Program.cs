using Ae.Galeriya.Piwigo;
using System.Threading;

namespace Ae.Piwigo.Console
{
    public static class Program
    {
        public static void Main()
        {
            var server = new PiwigoHttpServer();

            server.Listen(CancellationToken.None).GetAwaiter().GetResult();

            System.Console.WriteLine("wibble");
            System.Console.ReadLine();
        }
    }
}
