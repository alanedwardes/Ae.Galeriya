using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoLoginMethod : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SignInManager<IdentityUser> _signInManager;

        public string MethodName => "pwg.session.login";

        public PiwigoLoginMethod(IHttpContextAccessor contextAccessor, SignInManager<IdentityUser> signInManager)
        {
            _contextAccessor = contextAccessor;
            _signInManager = signInManager;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var form = _contextAccessor.HttpContext.Request.Form;

            var result = await _signInManager.PasswordSignInAsync(form["username"], form["password"], true, false);
            if (!result.Succeeded)
            {
                throw new Exception("Invalid username/password") { HResult = 401 };
            }

            //_contextAccessor.HttpContext.Response.Cookies.Append("session", "wibble");
            return true;
        }
    }
}
