using Ae.Galeriya.Piwigo.Entities;
using System.Collections.Generic;

namespace Ae.Galeriya.Piwigo
{
    internal interface IPiwigoImageDerivativesGenerator
    {
        IReadOnlyDictionary<string, PiwigoThumbnail> GenerateDerivatives(uint imageId);
    }
}