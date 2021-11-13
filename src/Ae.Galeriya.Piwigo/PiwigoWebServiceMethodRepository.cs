using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoWebServiceMethodRepository : IPiwigoWebServiceMethodRepository
    {
        private readonly IServiceProvider _serviceProvider;
        public PiwigoWebServiceMethodRepository(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
        public IEnumerable<string> GetMethods() => _serviceProvider.GetServices<IPiwigoWebServiceMethod>().Select(x => x.MethodName);
        public IPiwigoWebServiceMethod GetMethod(string methodName) => _serviceProvider.GetServices<IPiwigoWebServiceMethod>().SingleOrDefault(x => x.MethodName == methodName);
    }
}
