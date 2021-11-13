using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoLogoutMethod : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public string MethodName => "pwg.session.logout";

        public PiwigoLogoutMethod(IHttpContextAccessor contextAccessor) => _contextAccessor = contextAccessor;

        public Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete("session");
            return Task.FromResult<object>(true);
        }
    }
}
