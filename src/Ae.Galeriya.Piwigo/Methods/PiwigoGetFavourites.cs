using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetFavourites : IPiwigoWebServiceMethod
    {
        public string MethodName => "pwg.users.favorites.getList";
        public bool AllowAnonymous => false;

        public Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            return Task.FromResult<object>(new PiwigoImages
            {
                Pagination = new PiwigoPagination
                {
                    Page = 0,
                    PerPage = 0,
                    Count = 0,
                    TotalCount = 0
                },
                Images = new PiwigoImageSummary[0]
            });
        }
    }
}
