using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoLoginMethod : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public string MethodName => "pwg.session.login";

        public PiwigoLoginMethod(IHttpContextAccessor contextAccessor) => _contextAccessor = contextAccessor;

        public Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var form = _contextAccessor.HttpContext.Request.Form;

            _contextAccessor.HttpContext.Response.Cookies.Append("session", "wibble");
            return Task.FromResult<object>(true);
        }
    }
}
