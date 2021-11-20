using System;
using System.IO;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoConfiguration
    {
        Uri BaseAddress { get; set; }
        DirectoryInfo TempFolder { get; set; }
    }
}