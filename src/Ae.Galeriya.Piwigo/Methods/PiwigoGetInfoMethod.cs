using Ae.Galeriya.Piwigo.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetInfoMethod : IPiwigoWebServiceMethod
    {
        public string MethodName => "pwg.getInfos";
        public bool AllowAnonymous => false;

        public Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            return Task.FromResult<object>(new[]
            {
                new PiwigoInfoItem{Name = "version", Value="11.5.0"},
                new PiwigoInfoItem{Name = "nb_elements", Value="24"},
                new PiwigoInfoItem{Name = "nb_categories", Value="1"},
                new PiwigoInfoItem{Name = "nb_virtual", Value="1"},
                new PiwigoInfoItem{Name = "nb_physical", Value="0"},
                new PiwigoInfoItem{Name = "nb_image_category", Value="24"},
                new PiwigoInfoItem{Name = "nb_tags", Value="1"},
                new PiwigoInfoItem{Name = "nb_image_tag", Value="1"},
                new PiwigoInfoItem{Name = "nb_users", Value="2"},
                new PiwigoInfoItem{Name = "nb_groups", Value="1"},
                new PiwigoInfoItem{Name = "nb_comments", Value = "0"},
                new PiwigoInfoItem{Name = "first_date", Value = DateTimeOffset.UtcNow}
            });
        }
    }
}
