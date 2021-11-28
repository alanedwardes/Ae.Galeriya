using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoListMethod : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoWebServiceMethodRepository _methodRepository;

        public PiwigoListMethod(IPiwigoWebServiceMethodRepository methodRepository) => _methodRepository = methodRepository;

        public string MethodName => "reflection.getMethodList";
        public bool AllowAnonymous => true;

        public Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            return Task.FromResult<object>(new PiwigoMethods { Methods =  _methodRepository.GetMethods() });
        }
    }
}
