using Ae.Galeriya.Piwigo.Entities;
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
        public bool AllowAnonymous => true;

        public PiwigoLoginMethod(IHttpContextAccessor contextAccessor, SignInManager<IdentityUser> signInManager)
        {
            _contextAccessor = contextAccessor;
            _signInManager = signInManager;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var result = await _signInManager.PasswordSignInAsync(parameters["username"].ToString(), parameters["password"].ToString(), true, false);
            if (!result.Succeeded)
            {
                _contextAccessor.HttpContext.Response.StatusCode = 403;
                return new PiwigoResponse { Stat = "fail", Error = 403, Message = "Invalid username/password" };
            }

            return true;
        }
    }
}
