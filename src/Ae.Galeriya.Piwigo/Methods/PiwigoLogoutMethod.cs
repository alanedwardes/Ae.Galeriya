using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoLogoutMethod : IPiwigoWebServiceMethod
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public string MethodName => "pwg.session.logout";
        public bool AllowAnonymous => false;

        public PiwigoLogoutMethod(SignInManager<IdentityUser> signInManager) => _signInManager = signInManager;

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            await _signInManager.SignOutAsync();
            return true;
        }
    }
}
