using CommandLine;
using System;

namespace Ae.Galeriya.Console
{
    [Verb("serve", HelpText = "Serve the Piwigo endpoints")]
    public class ServeOptions
    {
        [Option("bucketName", Required = true, HelpText = "The bucket name")]
        public string BucketName { get; set; }
        [Option("baseAddress", Required = true, HelpText = "The base address")]
        public Uri BaseAddress { get; set; }
        [Option("bindAddress", Required = true, HelpText = "The address to bind to")]
        public string BindAddress { get; set; }
        [Option("memoryCacheSize", HelpText = "The memory cache size in bytes", Default = 512_000_000)]
        public long MemoryCacheSize { get; set; }
    }
}
