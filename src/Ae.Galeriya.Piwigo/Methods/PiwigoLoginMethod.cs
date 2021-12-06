using Ae.Galeriya.Core.Tables;
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
        private readonly SignInManager<User> _signInManager;

        public string MethodName => "pwg.session.login";
        public bool AllowAnonymous => true;

        public PiwigoLoginMethod(IHttpContextAccessor contextAccessor, SignInManager<User> signInManager)
        {
            _contextAccessor = contextAccessor;
            _signInManager = signInManager;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var result = await _signInManager.PasswordSignInAsync(parameters.GetRequired<string>("username"), parameters.GetRequired<string>("password"), true, false);
            if (!result.Succeeded)
            {
                _contextAccessor.HttpContext.Response.StatusCode = 400;
                return new PiwigoResponse { Stat = "fail", Error = 400, Message = "Invalid username/password" };
            }

            return true;
        }
    }
}
