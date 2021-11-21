using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using System.Collections.Generic;

namespace Ae.Galeriya.Piwigo
{
    internal interface IPiwigoPhotosPageGenerator
    {
        PiwigoImages CreatePage(int page, int perPage, IEnumerable<Photo> photos);
    }
}